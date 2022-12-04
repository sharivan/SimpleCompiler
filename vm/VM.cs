using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.RightsManagement;
using System.Threading;

using assembler;
using units;

namespace vm
{
    public enum SteppingMode
    {
        RUN,
        OVER,
        INTO,
        OUT,
        RUN_TO_IP
    }

    public class VM
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct StringRec
        {
            public IntPtr previous;
            public IntPtr next;
            public int refCount;
            public int len;
        }

        public static readonly int POINTER_SIZE = IntPtr.Size;
        public static readonly int STRING_SIZE = POINTER_SIZE;
        public static readonly int STRING_REC_SIZE = Marshal.SizeOf(typeof(StringRec));

        /*
         * Estrutura de uma string contada por referência
         * offset (32 bits)     offset (64 bits)    descrição
         * -16                  -24                 ponteiro para a string anteriormente alocada
         * -12                  -16                 ponteiro para a próxima string alocada
         * -8                   -8                  número de referências
         * -4                   -4                  tamanho da string (quantidade de caracteres, sem contar o caracter terminador nulo)
         * 0                    0                   primeiro caracter
         * 
         * O ponteiro para uma string contada por referência sempre apontara para o offset 0 que leva ao endereço do primeiro caracter,
         * isso torna esse tipo de string compatíveis com as strings de C. As strings contadas por referência também são terminadas com
         * um caracter nulo, da mesma forma que as strings em C.
         */

        private class ExternalFunctionEntry
        {
            public string functionName;
            public int functionIndex;
            public ExternalFunctionHandler handler;
            public int paramSize;

            public ExternalFunctionEntry(string functionName, int functionIndex, ExternalFunctionHandler handler, int paramSize)
            {
                this.functionName = functionName;
                this.functionIndex = functionIndex;
                this.handler = handler;
                this.paramSize = paramSize;
            }
        }

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void KernelCopyMemory(IntPtr dest, IntPtr src, int count);

        [DllImport("kernel32.dll", EntryPoint = "ZeroMemory", SetLastError = false)]
        public static extern void KernelZeroMemory(IntPtr ptr, int count);

        public delegate void ExternalFunctionHandler(VM vm);

        public const int DEFAULT_STACK_SIZE = 32798; // tamanho da pilha bytes

        public delegate void DisassemblyLine(int ip, string line);
        public delegate string ConsoleRead();
        public delegate void ConsolePrint(string message);
        public delegate void PauseDelegate(int ip);
        public delegate void SteppingDelegate(int ip, SteppingMode mode);
        public delegate void BreakpointDelegate(Breakpoint bp);

        private byte[] code;
        private IntPtr stack;

        // registradores
        private int ip; // ponteiro de instrução

        private int initialSP;
        private int lastIP;
        private int calls;
        private SteppingMode steppingMode = SteppingMode.RUN;
        private int runToIP = -1;
        private readonly List<Breakpoint> breakpoints;
        private readonly Dictionary<int, int> breakpointTable;

        private readonly List<ExternalFunctionEntry> externalFunctions;
        private readonly Dictionary<string, Tuple<int, int>> externalFunctionMapByName;
        private readonly Dictionary<int, string> externalFunctionMapByIndex;

        public event DisassemblyLine OnDisassemblyLine;
        public event ConsoleRead OnConsoleRead;
        public event ConsolePrint OnConsolePrint;
        public event PauseDelegate OnPause;
        public event SteppingDelegate OnStep;
        public event BreakpointDelegate OnBreakpoint;

        public int IP
        {
            get => ip;

            set => ip = value;
        }

        public int SP
        {
            get; set;
        }

        public int BP
        {
            get; set;
        }

        public bool Paused
        {
            get;
            private set;
        }

        public int CodeSize => code.Length;

        public int StackSize
        {
            get;
            private set;
        }

        public int StringCount
        {
            get;
            internal set;
        }

        public int AllocatedStringSize
        {
            get;
            private set;
        }

        public IntPtr LastString
        {
            get;
            private set;
        }

        public VM()
        {
            code = null;
            stack = IntPtr.Zero;
            StackSize = 0;

            breakpoints = new List<Breakpoint>();
            breakpointTable = new Dictionary<int, int>();

            externalFunctions = new List<ExternalFunctionEntry>();
            externalFunctionMapByName = new Dictionary<string, Tuple<int, int>>();
            externalFunctionMapByIndex = new Dictionary<int, string>();

            steppingMode = SteppingMode.RUN;
        }

        ~VM()
        {
            Free();
        }

        public void Initialize(Assembler assembler, int stackSize = DEFAULT_STACK_SIZE)
        {
            code = new byte[assembler.CodeSize];
            assembler.CopyCode(code);
            SP = 0;

            StackSize = stackSize + (int) assembler.ConstantSize;
            stack = Marshal.AllocHGlobal(StackSize);
            KernelZeroMemory(stack, StackSize);

            if (assembler.ConstantSize > 0)
            {
                byte[] constantBuffer = new byte[assembler.ConstantSize];
                assembler.CopyConstantBuffer(constantBuffer);
                Push(constantBuffer);
            }

            initialSP = SP;
            LastString = IntPtr.Zero;
            StringCount = 0;
            AllocatedStringSize = 0;

            breakpoints.Clear();
            breakpointTable.Clear();

            externalFunctions.Clear();
            externalFunctionMapByName.Clear();
            externalFunctionMapByIndex.Clear();

            for (int i = 0; i < assembler.ExternalFunctionCount; i++)
            {
                Tuple<string, int> entry = assembler.GetExternalFunction(i);
                AddExternalFunction(entry.Item1, i, entry.Item2);
            }

            foreach (var kv in UnitySystem.FUNCTIONS)
                BindExternalFunction(kv.Key, kv.Value);
        }

        public void Free()
        {
            code = null;

            SP = 0;
            ip = 0;
            BP = 0;

            if (stack != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(stack);
                stack = IntPtr.Zero;
                StackSize = 0;
            }

            FreeAllocatedStrings();

            breakpoints.Clear();
            breakpointTable.Clear();

            externalFunctions.Clear();
            externalFunctionMapByName.Clear();
            externalFunctionMapByIndex.Clear();
        }

        public IntPtr NewString(int len)
        {
            try
            {
                int size = STRING_REC_SIZE + (len + 1) * sizeof(char);
                IntPtr result = Marshal.AllocHGlobal(size);     
                result += STRING_REC_SIZE;

                unsafe
                {
                    var rec = (StringRec*) (result - STRING_REC_SIZE).ToPointer();
                    

                    rec->previous = LastString;
                    rec->next = IntPtr.Zero;

                    if (LastString != IntPtr.Zero)
                    {
                        var lastStringRec = (StringRec*) (LastString - STRING_REC_SIZE).ToPointer();
                        lastStringRec->next = result;
                    }

                    rec->refCount = 1;
                    rec->len = len;
                }
                
                WritePointer(result, '\0');
                LastString = result;

                AllocatedStringSize += size;
                StringCount++;
                return result;
            }
            catch (OutOfMemoryException)
            {
            }

            return IntPtr.Zero;
        }

        public IntPtr NewString(string s)
        {
            IntPtr result = NewString(s.Length);
            WritePointer(result, s);
            return result;
        }

        public static int StringLength(IntPtr str)
        {
            if (str == IntPtr.Zero)
                return 0;

            unsafe
            {
                var rec = (StringRec*) (str - STRING_REC_SIZE).ToPointer();
                return rec->len;
            }
        }

        public IntPtr SetStringLength(IntPtr str, int len)
        {
            if (str == IntPtr.Zero)
                return NewString(len);

            try
            {
                int oldSize = STRING_REC_SIZE + (StringLength(str) + 1) * sizeof(char);
                int newSize = STRING_REC_SIZE + (len + 1) * sizeof(char);

                str = Marshal.ReAllocHGlobal(str - STRING_REC_SIZE, (IntPtr) newSize);
                str += STRING_REC_SIZE;

                unsafe
                {
                    var rec = (StringRec*) (str - STRING_REC_SIZE).ToPointer();
                    rec->len = len;
                }

                WritePointer(str + len * sizeof(char), '\0');

                AllocatedStringSize -= oldSize;
                AllocatedStringSize += newSize;
                return str;
            }
            catch (OutOfMemoryException)
            {
            }

            return IntPtr.Zero;
        }

        public static void StringAddRef(IntPtr str)
        {
            if (str != IntPtr.Zero)
                unsafe
                {
                    var rec = (StringRec*) (str - STRING_REC_SIZE).ToPointer();
                    rec->refCount++;
                }
        }

        public IntPtr StringRelease(IntPtr str)
        {
            if (str == IntPtr.Zero)
                return IntPtr.Zero;

            unsafe
            {
                var rec = (StringRec*) (str - STRING_REC_SIZE).ToPointer();
                rec->refCount--;

                if (rec->refCount <= 0)
                {
                    int size = STRING_REC_SIZE + (StringLength(str) + 1) * sizeof(char);

                    IntPtr previous = rec->previous;
                    var previousRec = (StringRec*) (previous - STRING_REC_SIZE).ToPointer();

                    IntPtr next = rec->next;
                    var nextRec = (StringRec*) (next - STRING_REC_SIZE).ToPointer();

                    if (previous != IntPtr.Zero)
                        previousRec->next = next;

                    if (next != IntPtr.Zero)
                        nextRec->previous = previous;

                    if (str == LastString)
                        LastString = previous;

                    Marshal.FreeHGlobal(str - STRING_REC_SIZE);
                    AllocatedStringSize -= size;
                    return IntPtr.Zero;
                }
            }

            return str;
        }

        private void FreeAllocatedStrings()
        {
            while (LastString != IntPtr.Zero)
            {
                IntPtr str = LastString - STRING_REC_SIZE;
                LastString = ReadPointerPtr(str); // anterior
                Marshal.FreeHGlobal(str);
            }

            LastString = IntPtr.Zero;
            StringCount = 0;
            AllocatedStringSize = 0;
        }

        public Breakpoint AddBreakpoint(int ip, bool temporary = false, bool enabled = true)
        {
            lock (breakpoints)
            {
                Breakpoint result = GetBreakpoint(ip);
                if (result != null)
                {
                    result.enabled = enabled;

                    if (result.temporary && !temporary)
                        result.temporary = false;

                    return result;
                }

                int bpIP = ip;
                var opcode = (Opcode) ReadCodeByte(ref ip);
                result = new Breakpoint(bpIP, opcode, temporary, enabled);
                breakpoints.Add(result);
                breakpointTable.Add(bpIP, breakpoints.Count - 1);
                ip = bpIP;
                WriteCode(ref ip, (byte) Opcode.BREAK);
                return result;
            }
        }

        public Breakpoint GetBreakpoint(int ip)
        {
            lock (breakpoints)
            {
                return breakpointTable.TryGetValue(ip, out int index) ? breakpoints[index] : null;
            }
        }

        public Breakpoint ToggleBreakPoint(int ip)
        {
            Breakpoint bp = GetBreakpoint(ip);
            if (bp == null)
                return AddBreakpoint(ip);

            RemoveBreakpoint(bp);
            return bp;
        }

        public void RemoveBreakpoint(Breakpoint bp)
        {
            lock (breakpoints)
            {
                if (breakpoints.Remove(bp))
                {
                    Opcode opcode = bp.opcode;
                    int ip = bp.IP;

                    breakpointTable.Remove(ip);

                    WriteCode(ref ip, (byte) opcode);
                }
            }
        }

        public void RemoveBreakpoint(int ip)
        {
            lock (breakpoints)
            {
                if (breakpointTable.TryGetValue(ip, out int index))
                {
                    Breakpoint bp = breakpoints[index];
                    Opcode opcode = bp.opcode;

                    breakpointTable.Remove(ip);
                    breakpoints.RemoveAt(index);

                    WriteCode(ref ip, (byte) opcode);
                }
            }
        }

        public void ClearBreakpoints()
        {
            lock (breakpoints)
            {
                foreach (Breakpoint bp in breakpoints)
                {
                    Opcode opcode = bp.opcode;
                    int ip = bp.IP;
                    WriteCode(ref ip, (byte) opcode);
                }

                breakpoints.Clear();
                breakpointTable.Clear();
            }
        }

        public void AddExternalFunction(string functionName, int index, int paramSize)
        {
            if (externalFunctionMapByName.ContainsKey(functionName))
                throw new Exception("External function " + functionName + " already added.");

            if (index >= externalFunctions.Count)
                for (int i = 0; i <= index - externalFunctions.Count; i++)
                    externalFunctions.Add(null);

            externalFunctionMapByName.Add(functionName, new Tuple<int, int>(index, paramSize));
            externalFunctionMapByIndex.Add(index, functionName);
        }

        public void BindExternalFunction(string functionName, ExternalFunctionHandler function)
        {
            if (externalFunctionMapByName.TryGetValue(functionName, out Tuple<int, int> entry))
                externalFunctions[entry.Item1] = new ExternalFunctionEntry(functionName, entry.Item1, function, entry.Item2);
        }

        public byte ReadCodeByte(ref int ip) => code[ip++];

        private const int MASK0 = 0xff;
        private const int MASK1 = MASK0 << 8;
        private const int MASK2 = MASK0 << 16;
        private const int MASK3 = MASK0 << 24;
        private const long MASK4 = (long) MASK0 << 32;
        private const long MASK5 = (long) MASK0 << 40;
        private const long MASK6 = (long) MASK0 << 48;
        private const long MASK7 = (long) MASK0 << 56;

        public short ReadCodeShort(ref int ip)
        {
            int result = code[ip++] & MASK0;
            result |= (code[ip++] << 8) & MASK1;
            return (short) result;
        }

        public int ReadCodeInt(ref int ip)
        {
            int result = code[ip++] & MASK0;
            result |= (code[ip++] << 8) & MASK1;
            result |= (code[ip++] << 16) & MASK2;
            result |= (code[ip++] << 24) & MASK3;
            return result;
        }

        public long ReadCodeLong(ref int ip)
        {
            long result = code[ip++] & MASK0;
            result |= ((long) code[ip++] << 8) & MASK1;
            result |= ((long) code[ip++] << 16) & MASK2;
            result |= ((long) code[ip++] << 24) & MASK3;
            result |= ((long) code[ip++] << 32) & MASK4;
            result |= ((long) code[ip++] << 40) & MASK5;
            result |= ((long) code[ip++] << 48) & MASK6;
            result |= ((long) code[ip++] << 56) & MASK7;
            return result;
        }

        public IntPtr ReadCodePtr(ref int ip) => IntPtr.Size == sizeof(int) ? (IntPtr) ReadCodeInt(ref ip) : (IntPtr) ReadCodeLong(ref ip);

        public float ReadCodeFloat(ref int ip)
        {
            int value = ReadCodeInt(ref ip);
            return BitConverter.ToSingle(BitConverter.GetBytes(value), 0);
        }

        public double ReadCodeDouble(ref int ip)
        {
            long value = ReadCodeLong(ref ip);
            return BitConverter.ToDouble(BitConverter.GetBytes(value), 0);
        }

        public void WriteCode(ref int ip, byte value) => code[ip++] = value;

        public void WriteCode(ref int ip, short value)
        {
            code[ip++] = (byte) value;
            code[ip++] = (byte) (value >> 8);
        }

        public void WriteCode(ref int ip, int value)
        {
            code[ip++] = (byte) value;
            code[ip++] = (byte) (value >> 8);
            code[ip++] = (byte) (value >> 16);
            code[ip++] = (byte) (value >> 24);
        }

        public void WriteCode(ref int ip, long value)
        {
            code[ip++] = (byte) value;
            code[ip++] = (byte) (value >> 8);
            code[ip++] = (byte) (value >> 16);
            code[ip++] = (byte) (value >> 24);
            code[ip++] = (byte) (value >> 32);
            code[ip++] = (byte) (value >> 40);
            code[ip++] = (byte) (value >> 48);
            code[ip++] = (byte) (value >> 56);
        }

        public void WriteCode(ref int ip, float value) => WriteCode(ref ip, BitConverter.ToInt32(BitConverter.GetBytes(value), 0));

        public void WriteCode(ref int ip, double value) => WriteCode(ref ip, BitConverter.ToInt64(BitConverter.GetBytes(value), 0));

        public void WriteCode(ref int ip, string value)
        {
            byte[] bytes = new byte[value.Length * sizeof(char)];
            Buffer.BlockCopy(value.ToCharArray(), 0, bytes, 0, bytes.Length);
            WriteCode(ref ip, value);
            WriteCode(ref ip, (short) 0);
        }

        public void WriteCode(ref int ip, byte[] buf) => WriteCode(ref ip, buf, 0, buf.Length);

        public void WriteCode(ref int ip, byte[] buf, int off, int len)
        {
            Array.Copy(buf, off, code, ip, len);
            ip += len;
        }

        public byte ReadStackByte(int addr) => ReadPointerByte(ResidentToHostAddr(addr));

        public static byte ReadPointerByte(IntPtr addr)
        {
            unsafe
            {
                return *(byte*) addr;
            }
        }

        public char ReadStackChar(int addr) => ReadPointerChar(ResidentToHostAddr(addr));

        public static char ReadPointerChar(IntPtr addr)
        {
            unsafe
            {
                return *(char*) addr;
            }
        }

        public short ReadStackShort(int addr) => ReadPointerShort(ResidentToHostAddr(addr));

        public static short ReadPointerShort(IntPtr addr)
        {
            unsafe
            {
                return *(short*) addr;
            }
        }

        public int ReadStackInt(int addr) => ReadPointerInt(ResidentToHostAddr(addr));

        public static int ReadPointerInt(IntPtr addr)
        {
            unsafe
            {
                return *(int*) addr;
            }
        }

        public long ReadStackLong(int addr) => ReadPointerLong(ResidentToHostAddr(addr));

        public static long ReadPointerLong(IntPtr addr)
        {
            unsafe
            {
                return *(long*) addr;
            }
        }

        public IntPtr ReadStackPtr(int addr) => ReadPointerPtr(ResidentToHostAddr(addr));

        public static IntPtr ReadPointerPtr(IntPtr addr)
        {
            unsafe
            {
                return *(IntPtr*) addr;
            }
        }

        public float ReadStackFloat(int addr) => ReadPointerFloat(ResidentToHostAddr(addr));

        public static float ReadPointerFloat(IntPtr addr)
        {
            unsafe
            {
                return *(float*) addr;
            }
        }

        public double ReadStackDouble(int addr) => ReadPointerDouble(ResidentToHostAddr(addr));

        public static double ReadPointerDouble(IntPtr addr)
        {
            unsafe
            {
                return *(double*) addr;
            }
        }

        public string ReadStackString(int addr) => ReadPointerString(ResidentToHostAddr(addr));

        public static string ReadPointerString(IntPtr addr)
        {
            string result = "";
            while (true)
            {
                char c = (char) ReadPointerShort(addr);
                addr += sizeof(short);
                if (c == '\0')
                    break;

                result += c;
            }

            return result;
        }

        public void WriteStack(int addr, byte value) => WritePointer(ResidentToHostAddr(addr), value);

        public static void WritePointer(IntPtr addr, byte value)
        {
            unsafe
            {
                *(byte*) addr = value;
            }
        }

        public void WriteStack(int addr, char value) => WritePointer(ResidentToHostAddr(addr), value);

        public static void WritePointer(IntPtr addr, char value)
        {
            unsafe
            {
                *(char*) addr = value;
            }
        }

        public void WriteStack(int addr, short value) => WritePointer(ResidentToHostAddr(addr), value);

        public static void WritePointer(IntPtr addr, short value)
        {
            unsafe
            {
                *(short*) addr = value;
            }
        }

        public void WriteStack(int addr, int value) => WritePointer(ResidentToHostAddr(addr), value);

        public static void WritePointer(IntPtr addr, int value)
        {
            unsafe
            {
                *(int*) addr = value;
            }
        }

        public void WriteStack(int addr, long value) => WritePointer(ResidentToHostAddr(addr), value);

        public static void WritePointer(IntPtr addr, long value)
        {
            unsafe
            {
                *(long*) addr = value;
            }
        }

        public void WriteStack(int addr, IntPtr ptr) => WritePointer(ResidentToHostAddr(addr), ptr);

        public static void WritePointer(IntPtr addr, IntPtr value)
        {
            unsafe
            {
                *(IntPtr*) addr = value;
            }
        }

        public void WriteStack(int addr, float value) => WritePointer(ResidentToHostAddr(addr), value);

        public void WritePointer(IntPtr addr, float value)
        {
            unsafe
            {
                *(float*) addr = value;
            }
        }

        public void WriteStack(int addr, double value) => WritePointer(ResidentToHostAddr(addr), value);

        public static void WritePointer(IntPtr addr, double value)
        {
            unsafe
            {
                *(double*) addr = value;
            }
        }

        public void WriteStack(int addr, string value) => WritePointer(ResidentToHostAddr(addr), value);

        public static void WritePointer(IntPtr addr, string value)
        {
            byte[] bytes = new byte[value.Length * sizeof(char)];
            Buffer.BlockCopy(value.ToCharArray(), 0, bytes, 0, bytes.Length);
            WritePointer(addr, bytes);
            WritePointer(addr + bytes.Length, '\0');
        }

        public void WriteStack(int addr, byte[] buf) => WritePointer(ResidentToHostAddr(addr), buf);

        public static void WritePointer(IntPtr addr, byte[] buf) => WritePointer(addr, buf, 0, buf.Length);

        public void WriteStack(int addr, byte[] buf, int off, int len) => WritePointer(ResidentToHostAddr(addr), buf, off, len);

        public static void WritePointer(IntPtr addr, byte[] buf, int off, int len) => Marshal.Copy(buf, off, addr, len);

        public void MoveStackBlock(int srcAddr, int dstAddr, int len) => MovePointerBlock(ResidentToHostAddr(srcAddr), ResidentToHostAddr(dstAddr), len);

        public static void MovePointerBlock(IntPtr srcAddr, IntPtr dstAddr, int len) => KernelCopyMemory(dstAddr, srcAddr, len);

        public void MovePointerBlockToStack(IntPtr srcAddr, int dstAddr, int len) => MovePointerBlock(srcAddr, ResidentToHostAddr(dstAddr), len);

        public void MoveStackBlockToPointer(int srcAddr, IntPtr dstAddr, int len) => MovePointerBlock(ResidentToHostAddr(srcAddr), dstAddr, len);

        public void LoadStackBlock(int srcAddr, int len)
        {
            MoveStackBlock(srcAddr, SP, len);
            SP += len;
        }

        public void LoadPointerBlock(IntPtr srcAddr, int len)
        {
            MovePointerBlockToStack(srcAddr, SP, len);
            SP += len;
        }

        public void StoreStackBlock(int dstAddr, int len)
        {
            SP -= len;
            MoveStackBlock(SP, dstAddr, len);
        }

        public void StorePointerBlock(IntPtr dstAddr, int len)
        {
            SP -= len;
            MoveStackBlockToPointer(SP, dstAddr, len);
        }

        public int ReadStackTop() => ReadStackInt(SP - sizeof(int));

        public long ReadStackTop64() => ReadStackLong(SP - sizeof(long));

        public IntPtr ReadStackTopPtr() => ReadStackPtr(SP - IntPtr.Size);

        public float ReadStackTopFloat() => ReadStackFloat(SP - sizeof(float));

        public double ReadStackTopDouble() => ReadStackDouble(SP - sizeof(double));

        public void Push(int value)
        {
            WriteStack(SP, value);
            SP += sizeof(int);
        }

        public void Push(long value)
        {
            WriteStack(SP, value);
            SP += sizeof(long);
        }

        public void Push(IntPtr value)
        {
            WriteStack(SP, value);
            SP += IntPtr.Size;
        }

        public void Push(float value) => Push(BitConverter.ToInt32(BitConverter.GetBytes(value), 0));

        public void Push(double value) => Push(BitConverter.ToInt64(BitConverter.GetBytes(value), 0));

        public void Push(byte[] buf) => Push(buf, 0, buf.Length);

        public void Push(byte[] buf, int off, int len)
        {
            WriteStack(SP, buf, off, len);
            SP += len;
        }

        public int Pop()
        {
            SP -= sizeof(int);
            int result = ReadStackInt(SP);
            return result;
        }

        public long PopLong()
        {
            SP -= sizeof(long);
            long result = ReadStackLong(SP);
            return result;
        }

        public IntPtr PopPtr()
        {
            SP -= IntPtr.Size;
            IntPtr result = ReadStackPtr(SP);
            return result;
        }

        public float PopFloat()
        {
            int value = Pop();
            return BitConverter.ToSingle(BitConverter.GetBytes(value), 0);
        }

        public double PopDouble()
        {
            long value = PopLong();
            return BitConverter.ToDouble(BitConverter.GetBytes(value), 0);
        }

        public IntPtr ResidentToHostAddr(int addr) => stack + addr;

        public int HostToResidentAddr(IntPtr addr) => (int) ((nint) addr - stack);

        public bool IsStackHostAddr(IntPtr addr) => (nint) addr > stack && (nint) addr < stack + StackSize;

        private int GetParamAbsoluteOffset(int paramsSize, int index) => BP - 2 * sizeof(int) - paramsSize + index * sizeof(int);

        public IntPtr LoadParamAddr(int paramsSize, int index) => ResidentToHostAddr(GetParamAbsoluteOffset(paramsSize, index));

        public int LoadParam(int paramsSize, int index) => ReadStackInt(GetParamAbsoluteOffset(paramsSize, index));

        public long LoadParamLong(int paramsSize, int index) => ReadStackLong(GetParamAbsoluteOffset(paramsSize, index));

        public IntPtr LoadParamPtr(int paramsSize, int index) => ReadStackPtr(GetParamAbsoluteOffset(paramsSize, index));

        public float LoadParamFloat(int paramsSize, int index) => ReadStackFloat(GetParamAbsoluteOffset(paramsSize, index));

        public double LoadParamDouble(int paramsSize, int index) => ReadStackDouble(GetParamAbsoluteOffset(paramsSize, index));

        public void SetParam(int paramsSize, int index, int value) => WriteStack(GetParamAbsoluteOffset(paramsSize, index), value);

        public void SetParam(int paramsSize, int index, long value) => WriteStack(GetParamAbsoluteOffset(paramsSize, index), value);

        public void SetParam(int paramsSize, int index, IntPtr value) => WriteStack(GetParamAbsoluteOffset(paramsSize, index), value);

        public void SetParam(int paramsSize, int index, float value) => WriteStack(GetParamAbsoluteOffset(paramsSize, index), value);

        public void SetParam(int paramsSize, int index, double value) => WriteStack(GetParamAbsoluteOffset(paramsSize, index), value);

        public void Pause()
        {
            lock (breakpoints)
            {
                Paused = true;
            }
        }

        public void Resume()
        {
            lock (breakpoints)
            {
                steppingMode = SteppingMode.RUN;
                runToIP = -1;
                Paused = false;
                Monitor.PulseAll(breakpoints);
            }
        }

        public void StepOver()
        {
            lock (breakpoints)
            {
                steppingMode = SteppingMode.OVER;
                runToIP = -1;
                Paused = false;
                Monitor.PulseAll(breakpoints);
            }
        }

        public void StepInto()
        {
            lock (breakpoints)
            {
                steppingMode = SteppingMode.INTO;
                runToIP = -1;
                Paused = false;
                Monitor.PulseAll(breakpoints);
            }
        }

        public void StepReturn()
        {
            lock (breakpoints)
            {
                steppingMode = SteppingMode.OUT;
                runToIP = -1;
                Paused = false;
                Monitor.PulseAll(breakpoints);
            }
        }

        public void RunToIP(int ip)
        {
            lock (breakpoints)
            {
                steppingMode = SteppingMode.RUN_TO_IP;
                runToIP = ip;
                Paused = false;
                Monitor.PulseAll(breakpoints);
            }
        }

        private void Step()
        {
            lock (breakpoints)
            {
                Paused = true;

                OnStep?.Invoke(lastIP, steppingMode);

                while (Paused)
                    Monitor.Wait(breakpoints);
            }
        }

        private bool CheckPaused()
        {
            if (Paused || runToIP == lastIP)
            {
                lock (breakpoints)
                {
                    if (Paused || runToIP == lastIP)
                    {
                        Paused = true;
                        calls = 0;
                        runToIP = -1;
                        steppingMode = SteppingMode.RUN;

                        if (Paused)
                            OnPause?.Invoke(lastIP);
                        else
                            OnStep?.Invoke(lastIP, SteppingMode.RUN_TO_IP);

                        while (Paused)
                            Monitor.Wait(breakpoints);
                    }
                }

                return true;
            }

            return false;
        }

        private bool CheckBreak(ref Opcode opcode)
        {
            if (opcode == Opcode.BREAK)
            {
                lock (breakpoints)
                {
                    Breakpoint bp = GetBreakpoint(lastIP);
                    if (bp == null)
                        throw new Exception("Breakpoint perdido no ip " + lastIP + ". Abortando a execução do programa.");

                    opcode = bp.opcode;

                    if (OnBreakpoint != null)
                    {
                        calls = 0;
                        steppingMode = SteppingMode.RUN;
                        runToIP = -1;
                        Paused = true;

                        OnBreakpoint(bp);

                        while (Paused)
                            Monitor.Wait(breakpoints);

                        if (bp.Temporary)
                            RemoveBreakpoint(bp);
                    }
                }

                return true;
            }

            return false;
        }

        public void Run(bool stepOver = false, int runToIP = -1)
        {
            lock (breakpoints)
            {
                if (stepOver)
                {
                    this.runToIP = -1;
                    steppingMode = SteppingMode.OVER;
                }
                else if (runToIP != -1)
                {
                    this.runToIP = runToIP;
                    steppingMode = SteppingMode.RUN_TO_IP;
                }
                else
                {
                    this.runToIP = -1;
                    steppingMode = SteppingMode.RUN;
                }

                Paused = false;
            }

            SP = initialSP;
            ip = 0;
            BP = SP;
            calls = 0;

            FreeAllocatedStrings();
            KernelZeroMemory(stack + initialSP, StackSize - initialSP);

            try
            {
                while (ip < code.Length)
                {
                    SteppingMode oldSteppingMode = steppingMode;
                    switch (steppingMode)
                    {
                        case SteppingMode.RUN:
                        case SteppingMode.RUN_TO_IP:
                            while (ip < code.Length)
                            {
                                lastIP = ip;
                                int op = ReadCodeByte(ref ip) & 0xff;
                                var opcode = (Opcode) op;

                                if (!CheckPaused())
                                    CheckBreak(ref opcode);

                                if (!SingleStep(opcode))
                                    return;

                                if (steppingMode != oldSteppingMode)
                                    break;
                            }

                            break;

                        case SteppingMode.OVER:
                            while (ip < code.Length)
                            {
                                lastIP = ip;
                                int op = ReadCodeByte(ref ip) & 0xff;
                                var opcode = (Opcode) op;

                                if (calls == 0)
                                {
                                    this.runToIP = -1;
                                    Step();
                                }
                                else if (!CheckPaused())
                                    CheckBreak(ref opcode);

                                if (!SingleStep(opcode))
                                    return;

                                if (steppingMode != oldSteppingMode)
                                    break;
                            }

                            break;

                        case SteppingMode.INTO:
                            while (ip < code.Length)
                            {
                                lastIP = ip;
                                int op = ReadCodeByte(ref ip) & 0xff;
                                var opcode = (Opcode) op;

                                this.runToIP = -1;
                                calls = 0;
                                Step();

                                if (!SingleStep(opcode))
                                    return;

                                if (steppingMode != oldSteppingMode)
                                    break;
                            }

                            break;

                        case SteppingMode.OUT:
                            while (ip < code.Length)
                            {
                                lastIP = ip;
                                int op = ReadCodeByte(ref ip) & 0xff;
                                var opcode = (Opcode) op;

                                if (calls < 0)
                                {
                                    this.runToIP = -1;
                                    calls = 0;
                                    Step();
                                }
                                else if (!CheckPaused())
                                    CheckBreak(ref opcode);

                                if (!SingleStep(opcode))
                                    return;

                                if (steppingMode != oldSteppingMode)
                                    break;
                            }

                            break;
                    }
                }
            }
            finally
            {
                FreeAllocatedStrings();
            }
        }

        private bool SingleStep(Opcode opcode)
        {
            switch (opcode)
            {
                case Opcode.NOP:
                    break;

                case Opcode.LC8:
                {
                    int value = ReadCodeByte(ref ip);
                    Push(value);
                    break;
                }

                case Opcode.LC16:
                {
                    int value = ReadCodeShort(ref ip);
                    Push(value);
                    break;
                }

                case Opcode.LC32:
                {
                    int value = ReadCodeInt(ref ip);
                    Push(value);
                    break;
                }

                case Opcode.LC64:
                {
                    long value = ReadCodeLong(ref ip);
                    Push(value);
                    break;
                }

                case Opcode.LCPTR:
                {
                    IntPtr value = ReadCodePtr(ref ip);
                    Push(value);
                    break;
                }

                case Opcode.LIP:
                {
                    Push(ip);
                    break;
                }

                case Opcode.LSP:
                {
                    Push(SP);
                    break;
                }

                case Opcode.LBP:
                {
                    Push(BP);
                    break;
                }

                case Opcode.SIP:
                {
                    ip = Pop();
                    break;
                }

                case Opcode.SSP:
                {
                    SP = Pop();
                    break;
                }

                case Opcode.SBP:
                {
                    BP = Pop();
                    break;
                }

                case Opcode.ADDSP:
                {
                    int offset = ReadCodeInt(ref ip);
                    SP += offset;
                    break;
                }

                case Opcode.SUBSP:
                {
                    int offset = ReadCodeInt(ref ip);
                    SP -= offset;
                    break;
                }

                case Opcode.LHA:
                {
                    int offset = Pop();
                    Push(ResidentToHostAddr(offset));
                    break;
                }

                case Opcode.LGHA:
                {
                    int offset = ReadCodeInt(ref ip);
                    Push(ResidentToHostAddr(offset));
                    break;
                }

                case Opcode.LLHA:
                {
                    int offset = ReadCodeInt(ref ip);
                    Push(ResidentToHostAddr(BP + offset));
                    break;
                }

                case Opcode.LLRA:
                {
                    int offset = ReadCodeInt(ref ip);
                    Push(BP + offset);
                    break;
                }

                case Opcode.RHA:
                {
                    int offset = Pop();
                    Push(ResidentToHostAddr(offset));
                    break;
                }

                case Opcode.HRA:
                {
                    IntPtr addr = PopPtr();
                    Push(HostToResidentAddr(addr));
                    break;
                }

                case Opcode.LS8:
                {
                    int addr = Pop();
                    byte value = ReadStackByte(addr);
                    Push(value);
                    break;
                }

                case Opcode.LS16:
                {
                    int addr = Pop();
                    short value = ReadStackShort(addr);
                    Push(value);
                    break;
                }

                case Opcode.LS32:
                {
                    int addr = Pop();
                    int value = ReadStackInt(addr);
                    Push(value);
                    break;
                }

                case Opcode.LS64:
                {
                    int addr = Pop();
                    long value = ReadStackLong(addr);
                    Push(value);
                    break;
                }

                case Opcode.LSPTR:
                {
                    int addr = Pop();
                    IntPtr value = ReadStackPtr(addr);
                    Push(value);
                    break;
                }

                case Opcode.SS8:
                {
                    int value = Pop();
                    int addr = Pop();
                    WriteStack(addr, (byte) value);
                    break;
                }

                case Opcode.SS16:
                {
                    int value = Pop();
                    int addr = Pop();
                    WriteStack(addr, (short) value);
                    break;
                }

                case Opcode.SS32:
                {
                    int value = Pop();
                    int addr = Pop();
                    WriteStack(addr, value);
                    break;
                }

                case Opcode.SS64:
                {
                    long value = PopLong();
                    int addr = Pop();
                    WriteStack(addr, value);
                    break;
                }

                case Opcode.SSPTR:
                {
                    IntPtr value = PopPtr();
                    int addr = Pop();
                    WriteStack(addr, value);
                    break;
                }

                case Opcode.LG8:
                {
                    int offset = ReadCodeInt(ref ip);
                    byte value = ReadStackByte(offset);
                    Push(value);
                    break;
                }

                case Opcode.LG16:
                {
                    int offset = ReadCodeInt(ref ip);
                    short value = ReadStackShort(offset);
                    Push(value);
                    break;
                }

                case Opcode.LG32:
                {
                    int offset = ReadCodeInt(ref ip);
                    int value = ReadStackInt(offset);
                    Push(value);
                    break;
                }

                case Opcode.LG64:
                {
                    int offset = ReadCodeInt(ref ip);
                    long value = ReadStackLong(offset);
                    Push(value);
                    break;
                }

                case Opcode.LGPTR:
                {
                    int offset = ReadCodeInt(ref ip);
                    IntPtr value = ReadStackPtr(offset);
                    Push(value);
                    break;
                }

                case Opcode.LL8:
                {
                    int offset = ReadCodeInt(ref ip);
                    byte value = ReadStackByte(BP + offset);
                    Push(value);
                    break;
                }

                case Opcode.LL16:
                {
                    int offset = ReadCodeInt(ref ip);
                    short value = ReadStackShort(BP + offset);
                    Push(value);
                    break;
                }

                case Opcode.LL32:
                {
                    int offset = ReadCodeInt(ref ip);
                    int value = ReadStackInt(BP + offset);
                    Push(value);
                    break;
                }

                case Opcode.LL64:
                {
                    int offset = ReadCodeInt(ref ip);
                    long value = ReadStackLong(BP + offset);
                    Push(value);
                    break;
                }

                case Opcode.LLPTR:
                {
                    int offset = ReadCodeInt(ref ip);
                    IntPtr value = ReadStackPtr(BP + offset);
                    Push(value);
                    break;
                }

                case Opcode.SG8:
                {
                    int offset = ReadCodeInt(ref ip);
                    int value = Pop();
                    WriteStack(offset, (byte) value);
                    break;
                }

                case Opcode.SG16:
                {
                    int offset = ReadCodeInt(ref ip);
                    int value = Pop();
                    WriteStack(offset, (short) value);
                    break;
                }

                case Opcode.SG32:
                {
                    int offset = ReadCodeInt(ref ip);
                    int value = Pop();
                    WriteStack(offset, value);
                    break;
                }

                case Opcode.SG64:
                {
                    int offset = ReadCodeInt(ref ip);
                    long value = PopLong();
                    WriteStack(offset, value);
                    break;
                }

                case Opcode.SGPTR:
                {
                    int offset = ReadCodeInt(ref ip);
                    IntPtr value = PopPtr();
                    WriteStack(offset, value);
                    break;
                }

                case Opcode.SL8:
                {
                    int offset = ReadCodeInt(ref ip);
                    int value = Pop();
                    WriteStack(BP + offset, (byte) value);
                    break;
                }

                case Opcode.SL16:
                {
                    int offset = ReadCodeInt(ref ip);
                    int value = Pop();
                    WriteStack(BP + offset, (short) value);
                    break;
                }

                case Opcode.SL32:
                {
                    int offset = ReadCodeInt(ref ip);
                    int value = Pop();
                    WriteStack(BP + offset, value);
                    break;
                }

                case Opcode.SL64:
                {
                    int offset = ReadCodeInt(ref ip);
                    long value = PopLong();
                    WriteStack(BP + offset, value);
                    break;
                }

                case Opcode.SLPTR:
                {
                    int offset = ReadCodeInt(ref ip);
                    IntPtr value = PopPtr();
                    WriteStack(BP + offset, value);
                    break;
                }

                case Opcode.LPTR8:
                {
                    IntPtr addr = PopPtr();
                    byte result = ReadPointerByte(addr);
                    Push(result);
                    break;
                }

                case Opcode.LPTR16:
                {
                    IntPtr addr = PopPtr();
                    short result = ReadPointerShort(addr);
                    Push(result);
                    break;
                }

                case Opcode.LPTR32:
                {
                    IntPtr addr = PopPtr();
                    int result = ReadPointerInt(addr);
                    Push(result);
                    break;
                }

                case Opcode.LPTR64:
                {
                    IntPtr addr = PopPtr();
                    long result = ReadPointerLong(addr);
                    Push(result);
                    break;
                }

                case Opcode.LPTRPTR:
                {
                    IntPtr addr = PopPtr();
                    IntPtr result = ReadPointerPtr(addr);
                    Push(result);
                    break;
                }

                case Opcode.SPTR8:
                {
                    int value = Pop();
                    IntPtr addr = PopPtr();
                    WritePointer(addr, (byte) value);
                    break;
                }

                case Opcode.SPTR16:
                {
                    int value = Pop();
                    IntPtr addr = PopPtr();
                    WritePointer(addr, (short) value);
                    break;
                }

                case Opcode.SPTR32:
                {
                    int value = Pop();
                    IntPtr addr = PopPtr();
                    WritePointer(addr, value);
                    break;
                }

                case Opcode.SPTR64:
                {
                    long value = PopLong();
                    IntPtr addr = PopPtr();
                    WritePointer(addr, value);
                    break;
                }

                case Opcode.SPTRPTR:
                {
                    IntPtr value = PopPtr();
                    IntPtr addr = PopPtr();
                    WritePointer(addr, value);
                    break;
                }

                case Opcode.ADD:
                {
                    int operand2 = Pop();
                    int operand1 = Pop();
                    int result = operand1 + operand2;
                    Push(result);
                    break;
                }

                case Opcode.ADD64:
                {
                    long operand2 = PopLong();
                    long operand1 = PopLong();
                    long result = operand1 + operand2;
                    Push(result);
                    break;
                }

                case Opcode.SUB:
                {
                    int operand2 = Pop();
                    int operand1 = Pop();
                    int result = operand1 - operand2;
                    Push(result);
                    break;
                }

                case Opcode.SUB64:
                {
                    long operand2 = PopLong();
                    long operand1 = PopLong();
                    long result = operand1 - operand2;
                    Push(result);
                    break;
                }

                case Opcode.MUL:
                {
                    int operand2 = Pop();
                    int operand1 = Pop();
                    int result = operand1 * operand2;
                    Push(result);
                    break;
                }

                case Opcode.MUL64:
                {
                    long operand2 = PopLong();
                    long operand1 = PopLong();
                    long result = operand1 * operand2;
                    Push(result);
                    break;
                }

                case Opcode.DIV:
                {
                    int operand2 = Pop();
                    int operand1 = Pop();
                    int result = operand1 / operand2;
                    Push(result);
                    break;
                }

                case Opcode.DIV64:
                {
                    long operand2 = PopLong();
                    long operand1 = PopLong();
                    long result = operand1 / operand2;
                    Push(result);
                    break;
                }

                case Opcode.MOD:
                {
                    int operand2 = Pop();
                    int operand1 = Pop();
                    int result = operand1 % operand2;
                    Push(result);
                    break;
                }

                case Opcode.MOD64:
                {
                    long operand2 = PopLong();
                    long operand1 = PopLong();
                    long result = operand1 % operand2;
                    Push(result);
                    break;
                }

                case Opcode.NEG:
                {
                    int operand = Pop();
                    int result = -operand;
                    Push(result);
                    break;
                }

                case Opcode.NEG64:
                {
                    long operand = PopLong();
                    long result = -operand;
                    Push(result);
                    break;
                }

                case Opcode.AND:
                {
                    int operand2 = Pop();
                    int operand1 = Pop();
                    int result = operand1 & operand2;
                    Push(result);
                    break;
                }

                case Opcode.AND64:
                {
                    long operand2 = PopLong();
                    long operand1 = PopLong();
                    long result = operand1 & operand2;
                    Push(result);
                    break;
                }

                case Opcode.OR:
                {
                    int operand2 = Pop();
                    int operand1 = Pop();
                    int result = operand1 | operand2;
                    Push(result);
                    break;
                }

                case Opcode.OR64:
                {
                    long operand2 = PopLong();
                    long operand1 = PopLong();
                    long result = operand1 | operand2;
                    Push(result);
                    break;
                }

                case Opcode.XOR:
                {
                    int operand2 = Pop();
                    int operand1 = Pop();
                    int result = operand1 ^ operand2;
                    Push(result);
                    break;
                }

                case Opcode.XOR64:
                {
                    long operand2 = PopLong();
                    long operand1 = PopLong();
                    long result = operand1 ^ operand2;
                    Push(result);
                    break;
                }

                case Opcode.NOT:
                {
                    int operand = Pop();
                    int result = ~operand;
                    Push(result);
                    break;
                }

                case Opcode.NOT64:
                {
                    long operand = PopLong();
                    long result = ~operand;
                    Push(result);
                    break;
                }

                case Opcode.SHL:
                {
                    int operand2 = Pop();
                    int operand1 = Pop();
                    int result = operand1 << operand2;
                    Push(result);
                    break;
                }

                case Opcode.SHL64:
                {
                    int operand2 = Pop();
                    long operand1 = PopLong();
                    long result = operand1 << operand2;
                    Push(result);
                    break;
                }

                case Opcode.SHR:
                {
                    int operand2 = Pop();
                    int operand1 = Pop();
                    int result = operand1 >> operand2;
                    Push(result);
                    break;
                }

                case Opcode.SHR64:
                {
                    int operand2 = Pop();
                    long operand1 = PopLong();
                    long result = operand1 << operand2;
                    Push(result);
                    break;
                }

                case Opcode.USHR:
                {
                    int operand2 = Pop();
                    int operand1 = Pop();
                    uint result = ((uint) operand1) >> operand2;
                    Push(result);
                    break;
                }

                case Opcode.USHR64:
                {
                    int operand2 = Pop();
                    long operand1 = PopLong();
                    ulong result = ((ulong) operand1) >> operand2;
                    Push(result);
                    break;
                }

                case Opcode.FADD:
                {
                    float operand2 = PopFloat();
                    float operand1 = PopFloat();
                    float result = operand1 + operand2;
                    Push(result);
                    break;
                }

                case Opcode.FADD64:
                {
                    double operand2 = PopDouble();
                    double operand1 = PopDouble();
                    double result = operand1 + operand2;
                    Push(result);
                    break;
                }

                case Opcode.FSUB:
                {
                    float operand2 = PopFloat();
                    float operand1 = PopFloat();
                    float result = operand1 - operand2;
                    Push(result);
                    break;
                }

                case Opcode.FSUB64:
                {
                    double operand2 = PopDouble();
                    double operand1 = PopDouble();
                    double result = operand1 - operand2;
                    Push(result);
                    break;
                }

                case Opcode.FMUL:
                {
                    float operand2 = PopFloat();
                    float operand1 = PopFloat();
                    float result = operand1 * operand2;
                    Push(result);
                    break;
                }

                case Opcode.FMUL64:
                {
                    double operand2 = PopDouble();
                    double operand1 = PopDouble();
                    double result = operand1 * operand2;
                    Push(result);
                    break;
                }

                case Opcode.FDIV:
                {
                    float operand2 = PopFloat();
                    float operand1 = PopFloat();
                    float result = operand1 / operand2;
                    Push(result);
                    break;
                }

                case Opcode.FDIV64:
                {
                    double operand2 = PopDouble();
                    double operand1 = PopDouble();
                    double result = operand1 / operand2;
                    Push(result);
                    break;
                }

                case Opcode.FNEG:
                {
                    float operand = PopFloat();
                    float result = -operand;
                    Push(result);
                    break;
                }

                case Opcode.FNEG64:
                {
                    double operand = PopDouble();
                    double result = -operand;
                    Push(result);
                    break;
                }

                case Opcode.PTRADD:
                {
                    int operand2 = Pop();
                    IntPtr operand1 = PopPtr();
                    IntPtr result = operand1 + operand2;
                    Push(result);
                    break;
                }

                case Opcode.PTRSUB:
                {
                    int operand2 = Pop();
                    IntPtr operand1 = PopPtr();
                    IntPtr result = operand1 - operand2;
                    Push(result);
                    break;
                }

                case Opcode.I32I64:
                {
                    int operand = Pop();
                    long result = operand;
                    Push(result);
                    break;
                }

                case Opcode.I64I32:
                {
                    long operand = PopLong();
                    int result = (int) operand;
                    Push(result);
                    break;
                }

                case Opcode.I32F32:
                {
                    int operand = Pop();
                    float result = operand;
                    Push(result);
                    break;
                }

                case Opcode.I32F64:
                {
                    int operand = Pop();
                    double result = operand;
                    Push(result);
                    break;
                }

                case Opcode.I64F64:
                {
                    long operand = PopLong();
                    double result = operand;
                    Push(result);
                    break;
                }

                case Opcode.F32F64:
                {
                    float operand = PopFloat();
                    double result = operand;
                    Push(result);
                    break;
                }

                case Opcode.F32I32:
                {
                    float operand = PopFloat();
                    int result = (int) operand;
                    Push(result);
                    break;
                }

                case Opcode.F32I64:
                {
                    float operand = PopFloat();
                    long result = (long) operand;
                    Push(result);
                    break;
                }

                case Opcode.F64I64:
                {
                    double operand = PopDouble();
                    long result = (long) operand;
                    Push(result);
                    break;
                }

                case Opcode.F64F32:
                {
                    double operand = PopDouble();
                    float result = (float) operand;
                    Push(result);
                    break;
                }

                case Opcode.I32PTR:
                {
                    int operand = Pop();
                    var result = (IntPtr) operand;
                    Push(result);
                    break;
                }

                case Opcode.I64PTR:
                {
                    long operand = PopLong();
                    var result = (IntPtr) operand;
                    Push(result);
                    break;
                }

                case Opcode.PTRI32:
                {
                    IntPtr operand = PopPtr();
                    int result = (int) operand;
                    Push(result);
                    break;
                }

                case Opcode.PTRI64:
                {
                    IntPtr operand = PopPtr();
                    long result = (long) operand;
                    Push(result);
                    break;
                }

                case Opcode.CMPE:
                {
                    int operand2 = Pop();
                    int operand1 = Pop();
                    bool result = operand1 == operand2;
                    Push(result ? 1 : 0);
                    break;
                }

                case Opcode.CMPNE:
                {
                    int operand2 = Pop();
                    int operand1 = Pop();
                    bool result = operand1 != operand2;
                    Push(result ? 1 : 0);
                    break;
                }

                case Opcode.CMPG:
                {
                    int operand2 = Pop();
                    int operand1 = Pop();
                    bool result = operand1 > operand2;
                    Push(result ? 1 : 0);
                    break;
                }

                case Opcode.CMPGE:
                {
                    int operand2 = Pop();
                    int operand1 = Pop();
                    bool result = operand1 >= operand2;
                    Push(result ? 1 : 0);
                    break;
                }

                case Opcode.CMPL:
                {
                    int operand2 = Pop();
                    int operand1 = Pop();
                    bool result = operand1 < operand2;
                    Push(result ? 1 : 0);
                    break;
                }

                case Opcode.CMPLE:
                {
                    int operand2 = Pop();
                    int operand1 = Pop();
                    bool result = operand1 <= operand2;
                    Push(result ? 1 : 0);
                    break;
                }

                case Opcode.CMPE64:
                {
                    long operand2 = PopLong();
                    long operand1 = PopLong();
                    bool result = operand1 == operand2;
                    Push(result ? 1 : 0);
                    break;
                }

                case Opcode.CMPNE64:
                {
                    long operand2 = PopLong();
                    long operand1 = PopLong();
                    bool result = operand1 != operand2;
                    Push(result ? 1 : 0);
                    break;
                }

                case Opcode.CMPG64:
                {
                    long operand2 = PopLong();
                    long operand1 = PopLong();
                    bool result = operand1 > operand2;
                    Push(result ? 1 : 0);
                    break;
                }

                case Opcode.CMPGE64:
                {
                    long operand2 = PopLong();
                    long operand1 = PopLong();
                    bool result = operand1 >= operand2;
                    Push(result ? 1 : 0);
                    break;
                }

                case Opcode.CMPL64:
                {
                    long operand2 = PopLong();
                    long operand1 = PopLong();
                    bool result = operand1 < operand2;
                    Push(result ? 1 : 0);
                    break;
                }

                case Opcode.CMPLE64:
                {
                    long operand2 = PopLong();
                    long operand1 = PopLong();
                    bool result = operand1 <= operand2;
                    Push(result ? 1 : 0);
                    break;
                }

                case Opcode.FCMPE:
                {
                    float operand2 = PopFloat();
                    float operand1 = PopFloat();
                    bool result = operand1 == operand2;
                    Push(result ? 1 : 0);
                    break;
                }

                case Opcode.FCMPNE:
                {
                    float operand2 = PopFloat();
                    float operand1 = PopFloat();
                    bool result = operand1 != operand2;
                    Push(result ? 1 : 0);
                    break;
                }

                case Opcode.FCMPG:
                {
                    float operand2 = PopFloat();
                    float operand1 = PopFloat();
                    bool result = operand1 > operand2;
                    Push(result ? 1 : 0);
                    break;
                }

                case Opcode.FCMPGE:
                {
                    float operand2 = PopFloat();
                    float operand1 = PopFloat();
                    bool result = operand1 >= operand2;
                    Push(result ? 1 : 0);
                    break;
                }

                case Opcode.FCMPL:
                {
                    float operand2 = PopFloat();
                    float operand1 = PopFloat();
                    bool result = operand1 < operand2;
                    Push(result ? 1 : 0);
                    break;
                }

                case Opcode.FCMPLE:
                {
                    float operand2 = PopFloat();
                    float operand1 = PopFloat();
                    bool result = operand1 <= operand2;
                    Push(result ? 1 : 0);
                    break;
                }

                case Opcode.FCMPE64:
                {
                    double operand2 = PopDouble();
                    double operand1 = PopDouble();
                    bool result = operand1 == operand2;
                    Push(result ? 1 : 0);
                    break;
                }

                case Opcode.FCMPNE64:
                {
                    double operand2 = PopDouble();
                    double operand1 = PopDouble();
                    bool result = operand1 != operand2;
                    Push(result ? 1 : 0);
                    break;
                }

                case Opcode.FCMPG64:
                {
                    double operand2 = PopDouble();
                    double operand1 = PopDouble();
                    bool result = operand1 > operand2;
                    Push(result ? 1 : 0);
                    break;
                }

                case Opcode.FCMPGE64:
                {
                    double operand2 = PopDouble();
                    double operand1 = PopDouble();
                    bool result = operand1 >= operand2;
                    Push(result ? 1 : 0);
                    break;
                }

                case Opcode.FCMPL64:
                {
                    double operand2 = PopDouble();
                    double operand1 = PopDouble();
                    bool result = operand1 < operand2;
                    Push(result ? 1 : 0);
                    break;
                }

                case Opcode.FCMPLE64:
                {
                    double operand2 = PopDouble();
                    double operand1 = PopDouble();
                    bool result = operand1 <= operand2;
                    Push(result ? 1 : 0);
                    break;
                }

                case Opcode.CMPEPTR:
                {
                    IntPtr operand2 = PopPtr();
                    IntPtr operand1 = PopPtr();
                    bool result = operand1 == operand2;
                    Push(result ? 1 : 0);
                    break;
                }

                case Opcode.CMPNEPTR:
                {
                    IntPtr operand2 = PopPtr();
                    IntPtr operand1 = PopPtr();
                    bool result = operand1 != operand2;
                    Push(result ? 1 : 0);
                    break;
                }

                case Opcode.CMPGPTR:
                {
                    IntPtr operand2 = PopPtr();
                    IntPtr operand1 = PopPtr();

                    bool result = IntPtr.Size == sizeof(int) ? (int) operand1 > (int) operand2 : (long) operand1 > (long) operand2;

                    Push(result ? 1 : 0);
                    break;
                }

                case Opcode.CMPGEPTR:
                {
                    IntPtr operand2 = PopPtr();
                    IntPtr operand1 = PopPtr();

                    bool result = IntPtr.Size == sizeof(int) ? (int) operand1 >= (int) operand2 : (long) operand1 >= (long) operand2;

                    Push(result ? 1 : 0);
                    break;
                }

                case Opcode.CMPLPTR:
                {
                    IntPtr operand2 = PopPtr();
                    IntPtr operand1 = PopPtr();

                    bool result = IntPtr.Size == sizeof(int) ? (int) operand1 < (int) operand2 : (long) operand1 < (long) operand2;

                    Push(result ? 1 : 0);
                    break;
                }

                case Opcode.CMPLEPTR:
                {
                    IntPtr operand2 = PopPtr();
                    IntPtr operand1 = PopPtr();

                    bool result = IntPtr.Size == sizeof(int) ? (int) operand1 <= (int) operand2 : (long) operand1 <= (long) operand2;

                    Push(result ? 1 : 0);
                    break;
                }

                case Opcode.JMP:
                {
                    int offset = ReadCodeInt(ref ip);
                    ip = lastIP + offset;
                    break;
                }

                case Opcode.JT:
                {
                    int offset = ReadCodeInt(ref ip);
                    int value = Pop() & 1;

                    if (value == 1)
                        ip = lastIP + offset;

                    break;
                }

                case Opcode.JF:
                {
                    int offset = ReadCodeInt(ref ip);
                    int value = Pop() & 1;

                    if (value == 0)
                        ip = lastIP + offset;

                    break;
                }

                case Opcode.POP:
                {
                    Pop();
                    break;
                }

                case Opcode.POP2:
                {
                    PopLong();
                    break;
                }

                case Opcode.POPN:
                {
                    byte n = ReadCodeByte(ref ip);

                    for (int i = 0; i < n; i++)
                        Pop();

                    break;
                }

                case Opcode.DUP:
                {
                    int value = ReadStackTop();
                    Push(value);
                    break;
                }

                case Opcode.DUP64:
                {
                    long value = ReadStackTop64();
                    Push(value);
                    break;
                }

                case Opcode.DUPN:
                {
                    byte n = ReadCodeByte(ref ip);
                    int value = ReadStackTop();

                    for (int i = 0; i < n; i++)
                        Push(value);

                    break;
                }

                case Opcode.DUP64N:
                {
                    byte n = ReadCodeByte(ref ip);
                    long value = ReadStackTop64();

                    for (int i = 0; i < n; i++)
                        Push(value);

                    break;
                }

                case Opcode.CALL:
                {
                    int offset = ReadCodeInt(ref ip);
                    Push(ip);
                    Push(BP);
                    BP = SP;
                    ip = lastIP + offset;

                    if (steppingMode is SteppingMode.OVER or SteppingMode.OUT)
                        lock (breakpoints)
                        {
                            if (steppingMode is SteppingMode.OVER or SteppingMode.OUT)
                                calls++;
                        }

                    break;
                }

                case Opcode.ICALL:
                {
                    int offset = Pop();
                    Push(ip);
                    Push(BP);
                    BP = SP;
                    ip = lastIP + offset;

                    if (steppingMode is SteppingMode.OVER or SteppingMode.OUT)
                        lock (breakpoints)
                        {
                            if (steppingMode is SteppingMode.OVER or SteppingMode.OUT)
                                calls++;
                        }

                    break;
                }

                case Opcode.ECALL:
                {
                    int index = ReadCodeInt(ref ip);
                    ExternalFunctionEntry entry = externalFunctions[index];

                    Push(ip);
                    Push(BP);
                    BP = SP;

                    entry.handler(this);

                    SP = BP;
                    BP = Pop();
                    ip = Pop();
                    SP -= entry.paramSize;

                    break;
                }

                case Opcode.RET:
                {
                    BP = Pop();
                    ip = Pop();

                    if (steppingMode is SteppingMode.OVER or SteppingMode.OUT)
                        lock (breakpoints)
                        {
                            if (steppingMode is SteppingMode.OVER or SteppingMode.OUT)
                                calls--;
                        }

                    break;
                }

                case Opcode.RETN:
                {
                    int count = ReadCodeInt(ref ip);
                    BP = Pop();
                    ip = Pop();
                    SP -= count;

                    if (steppingMode is SteppingMode.OVER or SteppingMode.OUT)
                        lock (breakpoints)
                        {
                            if (steppingMode is SteppingMode.OVER or SteppingMode.OUT)
                                calls--;
                        }

                    break;
                }

                case Opcode.SCANB:
                {
                    IntPtr addr = PopPtr();
                    string str = ReadFromConsole();
                    try
                    {
                        if (str is "verdade" or "1")
                            WritePointer(addr, 1);
                        else if (str is "falso" or "0")
                            WritePointer(addr, 0);
                        else
                        {
                            bool value = bool.Parse(str);
                            WritePointer(addr, value ? 1 : 0);
                        }
                    }
                    catch (Exception e) when (e is FormatException or OverflowException)
                    {
                        WritePointer(addr, (byte) 0);
                    }

                    break;
                }

                case Opcode.SCAN8:
                {
                    IntPtr addr = PopPtr();
                    string str = ReadFromConsole();
                    try
                    {
                        int value = int.Parse(str, CultureInfo.InvariantCulture);
                        WritePointer(addr, (byte) value);
                    }
                    catch (Exception e) when (e is FormatException or OverflowException)
                    {
                        WritePointer(addr, (byte) 0);
                    }

                    break;
                }

                case Opcode.SCANC:
                {
                    IntPtr addr = PopPtr();
                    string str = ReadFromConsole();
                    if (str.Length == 0)
                        WritePointer(addr, (short) 0);
                    else
                        WritePointer(addr, (short) str[0]);

                    break;
                }

                case Opcode.SCAN16:
                {
                    IntPtr addr = PopPtr();
                    string str = ReadFromConsole();
                    try
                    {
                        int value = int.Parse(str, CultureInfo.InvariantCulture);
                        WritePointer(addr, (short) value);
                    }
                    catch (Exception e) when (e is FormatException or OverflowException)
                    {
                        WritePointer(addr, (short) 0);
                    }

                    break;
                }

                case Opcode.SCAN32:
                {
                    IntPtr addr = PopPtr();
                    string str = ReadFromConsole();
                    try
                    {
                        int value = int.Parse(str, CultureInfo.InvariantCulture);
                        WritePointer(addr, value);
                    }
                    catch (Exception e) when (e is FormatException or OverflowException)
                    {
                        WritePointer(addr, 0);
                    }

                    break;
                }

                case Opcode.SCAN64:
                {
                    IntPtr addr = PopPtr();
                    string str = ReadFromConsole();
                    try
                    {
                        long value = long.Parse(str, CultureInfo.InvariantCulture);
                        WritePointer(addr, value);
                    }
                    catch (Exception e) when (e is FormatException or OverflowException)
                    {
                        WritePointer(addr, 0L);
                    }

                    break;
                }

                case Opcode.FSCAN:
                {
                    IntPtr addr = PopPtr();
                    string str = ReadFromConsole();
                    try
                    {
                        float value = float.Parse(str, CultureInfo.InvariantCulture);
                        WritePointer(addr, value);
                    }
                    catch (Exception e) when (e is FormatException or OverflowException)
                    {
                        WritePointer(addr, 0F);
                    }

                    break;
                }

                case Opcode.FSCAN64:
                {
                    IntPtr addr = PopPtr();
                    string str = ReadFromConsole();
                    try
                    {
                        double value = double.Parse(str, CultureInfo.InvariantCulture);
                        WritePointer(addr, value);
                    }
                    catch (Exception e) when (e is FormatException or OverflowException)
                    {
                        WritePointer(addr, 0.0);
                    }

                    break;
                }

                case Opcode.SCANSTR:
                {
                    IntPtr addr = PopPtr();
                    string value = ReadFromConsole();
                    WritePointer(addr, value);
                    break;
                }

                case Opcode.PRINTB:
                {
                    int value = Pop();
                    Print((value & 1) != 0 ? "verdade" : "falso");
                    break;
                }

                case Opcode.PRINTC:
                {
                    int value = Pop();
                    Print(((char) value).ToString(CultureInfo.InvariantCulture));
                    break;
                }

                case Opcode.PRINT32:
                {
                    int value = Pop();
                    Print(value.ToString(CultureInfo.InvariantCulture));
                    break;
                }

                case Opcode.PRINT64:
                {
                    long value = PopLong();
                    Print(value.ToString(CultureInfo.InvariantCulture));
                    break;
                }

                case Opcode.FPRINT:
                {
                    float value = PopFloat();
                    Print(value.ToString(CultureInfo.InvariantCulture));
                    break;
                }

                case Opcode.FPRINT64:
                {
                    double value = PopDouble();
                    Print(value.ToString(CultureInfo.InvariantCulture));
                    break;
                }

                case Opcode.PRINTSTR:
                {
                    IntPtr addr = PopPtr();
                    string value = addr != IntPtr.Zero ? ReadPointerString(addr) : "nulo";
                    Print(value);
                    break;
                }

                case Opcode.HALT:
                    return false;

                default:
                    throw new Exception("Illegal Opcode " + opcode + " at IP " + lastIP);
            }

            return true;
        }

        private string ReadFromConsole() => OnConsoleRead != null ? OnConsoleRead() : Console.ReadLine();

        private static readonly char[] HEX_DIGITS = { 'a', 'b', 'c', 'd', 'e', 'f' };

        private string Format(int n, int digits)
        {
            string result = n.ToString();
            ;
            int diff = digits - result.Length;
            if (diff > 0)
                for (int i = 0; i < diff; i++)
                    result = "0" + result;

            return result;
        }

        private void PrintLn()
        {
            if (OnConsolePrint != null)
                OnConsolePrint("\n");
            else
                Console.WriteLine();
        }

        private void PrintLn(string message)
        {
            if (OnConsolePrint != null)
                OnConsolePrint(message + "\n");
            else
                Console.WriteLine(message);
        }

        private void Print(string message)
        {
            if (OnConsolePrint != null)
                OnConsolePrint(message);
            else
                Console.Write(message);
        }

        private void PrintDisassembledLine() => OnDisassemblyLine?.Invoke(-1, null);

        private void PrintDisassembledLine(int ip, int op, string s) => OnDisassemblyLine?.Invoke(ip, Format(ip, 8) + "  " + Format(op, 3) + "  " + s);

        public void Print()
        {
            int ip = 0;
            while (ip < code.Length)
            {
                int lastIP = ip;
                int op = ReadCodeByte(ref ip) & 0xff;
                var Opcode = (Opcode) op;

                switch (Opcode)
                {
                    case Opcode.NOP:
                        PrintDisassembledLine(lastIP, op, "NOP");
                        break;

                    case Opcode.LC8:
                    {
                        int value = ReadCodeByte(ref ip);
                        PrintDisassembledLine(lastIP, op, "LC8 " + value);
                        break;
                    }

                    case Opcode.LC16:
                    {
                        int value = ReadCodeShort(ref ip);
                        PrintDisassembledLine(lastIP, op, "LC16 " + value);
                        break;
                    }

                    case Opcode.LC32:
                    {
                        int value = ReadCodeInt(ref ip);
                        PrintDisassembledLine(lastIP, op, "LC32 " + value + " //" + BitConverter.ToSingle(BitConverter.GetBytes(value), 0).ToString(CultureInfo.InvariantCulture));
                        break;
                    }

                    case Opcode.LC64:
                    {
                        long value = ReadCodeLong(ref ip);
                        PrintDisassembledLine(lastIP, op, "LC64 " + value + " //" + BitConverter.ToDouble(BitConverter.GetBytes(value), 0).ToString(CultureInfo.InvariantCulture));
                        break;
                    }

                    case Opcode.LCPTR:
                    {
                        long value = ReadCodeLong(ref ip);
                        PrintDisassembledLine(lastIP, op, "LCPTR " + value + " //" + BitConverter.ToDouble(BitConverter.GetBytes(value), 0).ToString(CultureInfo.InvariantCulture));
                        break;
                    }

                    case Opcode.LIP:
                    {
                        PrintDisassembledLine(lastIP, op, "LIP");
                        break;
                    }

                    case Opcode.LSP:
                    {
                        PrintDisassembledLine(lastIP, op, "LSP");
                        break;
                    }

                    case Opcode.LBP:
                    {
                        PrintDisassembledLine(lastIP, op, "LBP");
                        break;
                    }

                    case Opcode.SIP:
                    {
                        PrintDisassembledLine(lastIP, op, "SIP");
                        break;
                    }

                    case Opcode.SSP:
                    {
                        PrintDisassembledLine(lastIP, op, "SSP");
                        break;
                    }

                    case Opcode.SBP:
                    {
                        PrintDisassembledLine(lastIP, op, "SBP");
                        break;
                    }

                    case Opcode.ADDSP:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintDisassembledLine(lastIP, op, "ADDSP " + offset);
                        break;
                    }

                    case Opcode.SUBSP:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintDisassembledLine(lastIP, op, "SUBSP " + offset);
                        break;
                    }

                    case Opcode.LGHA:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintDisassembledLine(lastIP, op, "LGHA " + offset);
                        break;
                    }

                    case Opcode.LLHA:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintDisassembledLine(lastIP, op, "LLHA " + offset + (offset < 0 ? " // param" : ""));
                        break;
                    }

                    case Opcode.LLRA:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintDisassembledLine(lastIP, op, "LLRA " + offset + (offset < 0 ? " // param" : ""));
                        break;
                    }

                    case Opcode.RHA:
                    {
                        PrintDisassembledLine(lastIP, op, "RHA");
                        break;
                    }

                    case Opcode.HRA:
                    {
                        PrintDisassembledLine(lastIP, op, "HRA");
                        break;
                    }

                    case Opcode.LS8:
                    {
                        PrintDisassembledLine(lastIP, op, "LS8");
                        break;
                    }

                    case Opcode.LS16:
                    {
                        PrintDisassembledLine(lastIP, op, "LS16");
                        break;
                    }

                    case Opcode.LS32:
                    {
                        PrintDisassembledLine(lastIP, op, "LS32");
                        break;
                    }

                    case Opcode.LS64:
                    {
                        PrintDisassembledLine(lastIP, op, "LS64");
                        break;
                    }

                    case Opcode.LSPTR:
                    {
                        PrintDisassembledLine(lastIP, op, "LSPTR");
                        break;
                    }

                    case Opcode.SS8:
                    {
                        PrintDisassembledLine(lastIP, op, "SS8");
                        break;
                    }

                    case Opcode.SS16:
                    {
                        PrintDisassembledLine(lastIP, op, "SS16");
                        break;
                    }

                    case Opcode.SS32:
                    {
                        PrintDisassembledLine(lastIP, op, "SS32");
                        break;
                    }

                    case Opcode.SS64:
                    {
                        PrintDisassembledLine(lastIP, op, "SS64");
                        break;
                    }

                    case Opcode.SSPTR:
                    {
                        PrintDisassembledLine(lastIP, op, "SSPTR");
                        break;
                    }

                    case Opcode.LG8:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintDisassembledLine(lastIP, op, "LG8 " + offset);
                        break;
                    }

                    case Opcode.LG16:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintDisassembledLine(lastIP, op, "LG16 " + offset);
                        break;
                    }

                    case Opcode.LG32:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintDisassembledLine(lastIP, op, "LG32 " + offset);
                        break;
                    }

                    case Opcode.LG64:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintDisassembledLine(lastIP, op, "LG64 " + offset);
                        break;
                    }

                    case Opcode.LGPTR:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintDisassembledLine(lastIP, op, "LGPTR " + offset);
                        break;
                    }

                    case Opcode.LL8:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintDisassembledLine(lastIP, op, "LL8 " + offset + (offset < 0 ? " // param" : ""));
                        break;
                    }

                    case Opcode.LL16:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintDisassembledLine(lastIP, op, "LL16 " + offset + (offset < 0 ? " // param" : ""));
                        break;
                    }

                    case Opcode.LL32:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintDisassembledLine(lastIP, op, "LL32 " + offset + (offset < 0 ? " // param" : ""));
                        break;
                    }

                    case Opcode.LL64:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintDisassembledLine(lastIP, op, "LL64 " + offset + (offset < 0 ? " // param" : ""));
                        break;
                    }

                    case Opcode.LLPTR:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintDisassembledLine(lastIP, op, "LLPTR " + offset + (offset < 0 ? " // param" : ""));
                        break;
                    }

                    case Opcode.SG8:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintDisassembledLine(lastIP, op, "SG8 " + offset);
                        break;
                    }

                    case Opcode.SG16:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintDisassembledLine(lastIP, op, "SG16 " + offset);
                        break;
                    }

                    case Opcode.SG32:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintDisassembledLine(lastIP, op, "SG32 " + offset);
                        break;
                    }

                    case Opcode.SG64:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintDisassembledLine(lastIP, op, "SG64 " + offset);
                        break;
                    }

                    case Opcode.SGPTR:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintDisassembledLine(lastIP, op, "SGPTR " + offset);
                        break;
                    }

                    case Opcode.SL8:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintDisassembledLine(lastIP, op, "SL8 " + offset + (offset < 0 ? " // param" : ""));
                        break;
                    }

                    case Opcode.SL16:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintDisassembledLine(lastIP, op, "SL16 " + offset + (offset < 0 ? " // param" : ""));
                        break;
                    }

                    case Opcode.SL32:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintDisassembledLine(lastIP, op, "SL32 " + offset + (offset < 0 ? " // param" : ""));
                        break;
                    }

                    case Opcode.SL64:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintDisassembledLine(lastIP, op, "SL64 " + offset + (offset < 0 ? " // param" : ""));
                        break;
                    }

                    case Opcode.SLPTR:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintDisassembledLine(lastIP, op, "SLPTR " + offset + (offset < 0 ? " // param" : ""));
                        break;
                    }

                    case Opcode.LPTR8:
                    {
                        PrintDisassembledLine(lastIP, op, "LPTR8");
                        break;
                    }

                    case Opcode.LPTR16:
                    {
                        PrintDisassembledLine(lastIP, op, "LPTR16");
                        break;
                    }

                    case Opcode.LPTR32:
                    {
                        PrintDisassembledLine(lastIP, op, "LPTR32");
                        break;
                    }

                    case Opcode.LPTR64:
                    {
                        PrintDisassembledLine(lastIP, op, "LPTR64");
                        break;
                    }

                    case Opcode.LPTRPTR:
                    {
                        PrintDisassembledLine(lastIP, op, "LPTRPTR");
                        break;
                    }

                    case Opcode.SPTR8:
                    {
                        PrintDisassembledLine(lastIP, op, "SPTR8");
                        break;
                    }

                    case Opcode.SPTR16:
                    {
                        PrintDisassembledLine(lastIP, op, "SPTR16");
                        break;
                    }

                    case Opcode.SPTR32:
                    {
                        PrintDisassembledLine(lastIP, op, "SPTR32");
                        break;
                    }

                    case Opcode.SPTR64:
                    {
                        PrintDisassembledLine(lastIP, op, "SPTR64");
                        break;
                    }

                    case Opcode.SPTRPTR:
                    {
                        PrintDisassembledLine(lastIP, op, "SPTRPTR");
                        break;
                    }

                    case Opcode.ADD:
                    {
                        PrintDisassembledLine(lastIP, op, "ADD");
                        break;
                    }

                    case Opcode.ADD64:
                    {
                        PrintDisassembledLine(lastIP, op, "ADD64");
                        break;
                    }

                    case Opcode.SUB:
                    {
                        PrintDisassembledLine(lastIP, op, "SUB");
                        break;
                    }

                    case Opcode.SUB64:
                    {
                        PrintDisassembledLine(lastIP, op, "SUB64");
                        break;
                    }

                    case Opcode.MUL:
                    {
                        PrintDisassembledLine(lastIP, op, "MUL");
                        break;
                    }

                    case Opcode.MUL64:
                    {
                        PrintDisassembledLine(lastIP, op, "MUL64");
                        break;
                    }

                    case Opcode.DIV:
                    {
                        PrintDisassembledLine(lastIP, op, "DIV");
                        break;
                    }

                    case Opcode.DIV64:
                    {
                        PrintDisassembledLine(lastIP, op, "DIV64");
                        break;
                    }

                    case Opcode.MOD:
                    {
                        PrintDisassembledLine(lastIP, op, "MOD");
                        break;
                    }

                    case Opcode.MOD64:
                    {
                        PrintDisassembledLine(lastIP, op, "MOD64");
                        break;
                    }

                    case Opcode.NEG:
                    {
                        PrintDisassembledLine(lastIP, op, "NEG");
                        break;
                    }

                    case Opcode.NEG64:
                    {
                        PrintDisassembledLine(lastIP, op, "NEG64");
                        break;
                    }

                    case Opcode.AND:
                    {
                        PrintDisassembledLine(lastIP, op, "AND");
                        break;
                    }

                    case Opcode.AND64:
                    {
                        PrintDisassembledLine(lastIP, op, "AND64");
                        break;
                    }

                    case Opcode.OR:
                    {
                        PrintDisassembledLine(lastIP, op, "OR");
                        break;
                    }

                    case Opcode.OR64:
                    {
                        PrintDisassembledLine(lastIP, op, "OR64");
                        break;
                    }

                    case Opcode.XOR:
                    {
                        PrintDisassembledLine(lastIP, op, "XOR");
                        break;
                    }

                    case Opcode.XOR64:
                    {
                        PrintDisassembledLine(lastIP, op, "XOR64");
                        break;
                    }

                    case Opcode.NOT:
                    {
                        PrintDisassembledLine(lastIP, op, "NOT");
                        break;
                    }

                    case Opcode.NOT64:
                    {
                        PrintDisassembledLine(lastIP, op, "NOT64");
                        break;
                    }

                    case Opcode.SHL:
                    {
                        PrintDisassembledLine(lastIP, op, "SHL");
                        break;
                    }

                    case Opcode.SHL64:
                    {
                        PrintDisassembledLine(lastIP, op, "SHL64");
                        break;
                    }

                    case Opcode.SHR:
                    {
                        PrintDisassembledLine(lastIP, op, "SHR");
                        break;
                    }

                    case Opcode.SHR64:
                    {
                        PrintDisassembledLine(lastIP, op, "SHR64");
                        break;
                    }

                    case Opcode.USHR:
                    {
                        PrintDisassembledLine(lastIP, op, "USHR");
                        break;
                    }

                    case Opcode.USHR64:
                    {
                        PrintDisassembledLine(lastIP, op, "USHR64");
                        break;
                    }

                    case Opcode.FADD:
                    {
                        PrintDisassembledLine(lastIP, op, "FADD");
                        break;
                    }

                    case Opcode.FADD64:
                    {
                        PrintDisassembledLine(lastIP, op, "FADD64");
                        break;
                    }

                    case Opcode.FSUB:
                    {
                        PrintDisassembledLine(lastIP, op, "FSUB");
                        break;
                    }

                    case Opcode.FSUB64:
                    {
                        PrintDisassembledLine(lastIP, op, "FSUB64");
                        break;
                    }

                    case Opcode.FMUL:
                    {
                        PrintDisassembledLine(lastIP, op, "FMUL");
                        break;
                    }

                    case Opcode.FMUL64:
                    {
                        PrintDisassembledLine(lastIP, op, "FMUL64");
                        break;
                    }

                    case Opcode.FDIV:
                    {
                        PrintDisassembledLine(lastIP, op, "FDIV");
                        break;
                    }

                    case Opcode.FDIV64:
                    {
                        PrintDisassembledLine(lastIP, op, "FDIV64");
                        break;
                    }

                    case Opcode.FNEG:
                    {
                        PrintDisassembledLine(lastIP, op, "FNEG");
                        break;
                    }

                    case Opcode.FNEG64:
                    {
                        PrintDisassembledLine(lastIP, op, "FNEG64");
                        break;
                    }

                    case Opcode.PTRADD:
                    {
                        PrintDisassembledLine(lastIP, op, "PTRADD");
                        break;
                    }

                    case Opcode.PTRADD64:
                    {
                        PrintDisassembledLine(lastIP, op, "PTRADD64");
                        break;
                    }

                    case Opcode.I32I64:
                    {
                        PrintDisassembledLine(lastIP, op, "I32I64");
                        break;
                    }

                    case Opcode.I64I32:
                    {
                        PrintDisassembledLine(lastIP, op, "I64I32");
                        break;
                    }

                    case Opcode.I32F32:
                    {
                        PrintDisassembledLine(lastIP, op, "I32F32");
                        break;
                    }

                    case Opcode.I32F64:
                    {
                        PrintDisassembledLine(lastIP, op, "I32F64");
                        break;
                    }

                    case Opcode.I64F64:
                    {
                        PrintDisassembledLine(lastIP, op, "I64F64");
                        break;
                    }

                    case Opcode.F32F64:
                    {
                        PrintDisassembledLine(lastIP, op, "F32F64");
                        break;
                    }

                    case Opcode.F32I32:
                    {
                        PrintDisassembledLine(lastIP, op, "F32I32");
                        break;
                    }

                    case Opcode.F32I64:
                    {
                        PrintDisassembledLine(lastIP, op, "F32I64");
                        break;
                    }

                    case Opcode.F64I64:
                    {
                        PrintDisassembledLine(lastIP, op, "F64I64");
                        break;
                    }

                    case Opcode.F64F32:
                    {
                        PrintDisassembledLine(lastIP, op, "F64F32");
                        break;
                    }

                    case Opcode.I32PTR:
                    {
                        PrintDisassembledLine(lastIP, op, "I32PTR");
                        break;
                    }

                    case Opcode.I64PTR:
                    {
                        PrintDisassembledLine(lastIP, op, "I64PTR");
                        break;
                    }

                    case Opcode.PTRI32:
                    {
                        PrintDisassembledLine(lastIP, op, "PTRI32");
                        break;
                    }

                    case Opcode.PTRI64:
                    {
                        PrintDisassembledLine(lastIP, op, "PTRI64");
                        break;
                    }

                    case Opcode.CMPE:
                    {
                        PrintDisassembledLine(lastIP, op, "CMPE");
                        break;
                    }

                    case Opcode.CMPNE:
                    {
                        PrintDisassembledLine(lastIP, op, "CMPNE");
                        break;
                    }

                    case Opcode.CMPG:
                    {
                        PrintDisassembledLine(lastIP, op, "CMPG");
                        break;
                    }

                    case Opcode.CMPGE:
                    {
                        PrintDisassembledLine(lastIP, op, "CMPGE");
                        break;
                    }

                    case Opcode.CMPL:
                    {
                        PrintDisassembledLine(lastIP, op, "CMPL");
                        break;
                    }

                    case Opcode.CMPLE:
                    {
                        PrintDisassembledLine(lastIP, op, "CMPLE");
                        break;
                    }

                    case Opcode.CMPE64:
                    {
                        PrintDisassembledLine(lastIP, op, "CMPE64");
                        break;
                    }

                    case Opcode.CMPNE64:
                    {
                        PrintDisassembledLine(lastIP, op, "CMPNE64");
                        break;
                    }

                    case Opcode.CMPG64:
                    {
                        PrintDisassembledLine(lastIP, op, "CMPG64");
                        break;
                    }

                    case Opcode.CMPGE64:
                    {
                        PrintDisassembledLine(lastIP, op, "CMPGE64");
                        break;
                    }

                    case Opcode.CMPL64:
                    {
                        PrintDisassembledLine(lastIP, op, "CMPL64");
                        break;
                    }

                    case Opcode.CMPLE64:
                    {
                        PrintDisassembledLine(lastIP, op, "CMPLE64");
                        break;
                    }

                    case Opcode.FCMPE:
                    {
                        PrintDisassembledLine(lastIP, op, "FCMPE");
                        break;
                    }

                    case Opcode.FCMPNE:
                    {
                        PrintDisassembledLine(lastIP, op, "FCMPNE");
                        break;
                    }

                    case Opcode.FCMPG:
                    {
                        PrintDisassembledLine(lastIP, op, "FCMPG");
                        break;
                    }

                    case Opcode.FCMPGE:
                    {
                        PrintDisassembledLine(lastIP, op, "FCMPGE");
                        break;
                    }

                    case Opcode.FCMPL:
                    {
                        PrintDisassembledLine(lastIP, op, "FCMPL");
                        break;
                    }

                    case Opcode.FCMPLE:
                    {
                        PrintDisassembledLine(lastIP, op, "FCMPLE");
                        break;
                    }

                    case Opcode.FCMPE64:
                    {
                        PrintDisassembledLine(lastIP, op, "FCMPE64");
                        break;
                    }

                    case Opcode.FCMPNE64:
                    {
                        PrintDisassembledLine(lastIP, op, "FCMPNE64");
                        break;
                    }

                    case Opcode.FCMPG64:
                    {
                        PrintDisassembledLine(lastIP, op, "FCMPG64");
                        break;
                    }

                    case Opcode.FCMPGE64:
                    {
                        PrintDisassembledLine(lastIP, op, "FCMPGE64");
                        break;
                    }

                    case Opcode.FCMPL64:
                    {
                        PrintDisassembledLine(lastIP, op, "FCMPL64");
                        break;
                    }

                    case Opcode.FCMPLE64:
                    {
                        PrintDisassembledLine(lastIP, op, "FCMPLE64");
                        break;
                    }

                    case Opcode.CMPEPTR:
                    {
                        PrintDisassembledLine(lastIP, op, "CMPEPTR");
                        break;
                    }

                    case Opcode.CMPNEPTR:
                    {
                        PrintDisassembledLine(lastIP, op, "CMPNEPTR");
                        break;
                    }

                    case Opcode.CMPGPTR:
                    {
                        PrintDisassembledLine(lastIP, op, "CMPGPTR");
                        break;
                    }

                    case Opcode.CMPGEPTR:
                    {
                        PrintDisassembledLine(lastIP, op, "CMPGEPTR");
                        break;
                    }

                    case Opcode.CMPLPTR:
                    {
                        PrintDisassembledLine(lastIP, op, "CMPLPTR");
                        break;
                    }

                    case Opcode.CMPLEPTR:
                    {
                        PrintDisassembledLine(lastIP, op, "CMPLEPTR");
                        break;
                    }

                    case Opcode.JMP:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintDisassembledLine(lastIP, op, "JMP " + Format(lastIP + offset, 8) + " //" + (offset > 0 ? "+" : "") + offset);
                        break;
                    }

                    case Opcode.JT:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintDisassembledLine(lastIP, op, "JT " + Format(lastIP + offset, 8) + " //" + (offset > 0 ? "+" : "") + offset);
                        break;
                    }

                    case Opcode.JF:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintDisassembledLine(lastIP, op, "JF " + Format(lastIP + offset, 8) + " //" + (offset > 0 ? "+" : "") + offset);
                        break;
                    }

                    case Opcode.POP:
                    {
                        PrintDisassembledLine(lastIP, op, "POP");
                        break;
                    }

                    case Opcode.POP2:
                    {
                        PrintDisassembledLine(lastIP, op, "POP2");
                        break;
                    }

                    case Opcode.POPN:
                    {
                        byte n = ReadCodeByte(ref ip);
                        PrintDisassembledLine(lastIP, op, "POPN " + n);
                        break;
                    }

                    case Opcode.DUP:
                    {
                        PrintDisassembledLine(lastIP, op, "DUP");
                        break;
                    }

                    case Opcode.DUP64:
                    {
                        PrintDisassembledLine(lastIP, op, "DUP64");
                        break;
                    }

                    case Opcode.DUPN:
                    {
                        byte n = ReadCodeByte(ref ip);
                        PrintDisassembledLine(lastIP, op, "DUPN " + n);
                        break;
                    }

                    case Opcode.DUP64N:
                    {
                        byte n = ReadCodeByte(ref ip);
                        PrintDisassembledLine(lastIP, op, "DUP64N " + n);
                        break;
                    }

                    case Opcode.CALL:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintDisassembledLine(lastIP, op, "CALL " + Format(lastIP + offset, 8) + " //" + (offset > 0 ? "+" : "") + offset);
                        break;
                    }

                    case Opcode.ICALL:
                    {
                        PrintDisassembledLine(lastIP, op, "ICALL");
                        break;
                    }

                    case Opcode.ECALL:
                    {
                        int index = ReadCodeInt(ref ip);
                        PrintDisassembledLine(lastIP, op, "ECALL " + (index >= 0 && index < externalFunctions.Count ? externalFunctions[index].functionName : "?") + " //" + index);
                        break;
                    }

                    case Opcode.RET:
                    {
                        PrintDisassembledLine(lastIP, op, "RET");
                        PrintDisassembledLine();
                        break;
                    }

                    case Opcode.RETN:
                    {
                        int count = ReadCodeInt(ref ip);
                        PrintDisassembledLine(lastIP, op, "RETN " + count);
                        PrintDisassembledLine();
                        break;
                    }

                    case Opcode.SCAN8:
                    {
                        PrintDisassembledLine(lastIP, op, "SCAN8");
                        break;
                    }

                    case Opcode.SCAN16:
                    {
                        PrintDisassembledLine(lastIP, op, "SCAN16");
                        break;
                    }

                    case Opcode.SCAN32:
                    {
                        PrintDisassembledLine(lastIP, op, "SCAN32");
                        break;
                    }

                    case Opcode.SCAN64:
                    {
                        PrintDisassembledLine(lastIP, op, "SCAN64");
                        break;
                    }

                    case Opcode.FSCAN:
                    {
                        PrintDisassembledLine(lastIP, op, "FSCAN");
                        break;
                    }

                    case Opcode.FSCAN64:
                    {
                        PrintDisassembledLine(lastIP, op, "FSCAN64");
                        break;
                    }

                    case Opcode.SCANSTR:
                    {
                        PrintDisassembledLine(lastIP, op, "SCANSTR");
                        break;
                    }

                    case Opcode.PRINTB:
                    {
                        PrintDisassembledLine(lastIP, op, "PRINTCB");
                        break;
                    }

                    case Opcode.PRINTC:
                    {
                        PrintDisassembledLine(lastIP, op, "PRINTC");
                        break;
                    }

                    case Opcode.PRINT32:
                    {
                        PrintDisassembledLine(lastIP, op, "PRINT32");
                        break;
                    }

                    case Opcode.PRINT64:
                    {
                        PrintDisassembledLine(lastIP, op, "PRINT64");
                        break;
                    }

                    case Opcode.FPRINT:
                    {
                        PrintDisassembledLine(lastIP, op, "FPRINT");
                        break;
                    }

                    case Opcode.FPRINT64:
                    {
                        PrintDisassembledLine(lastIP, op, "FPRINT64");
                        break;
                    }

                    case Opcode.PRINTSTR:
                    {
                        PrintDisassembledLine(lastIP, op, "PRINTSTR");
                        break;
                    }

                    case Opcode.HALT:
                    {
                        PrintDisassembledLine(lastIP, op, "HALT");
                        PrintDisassembledLine();
                        break;
                    }

                    case Opcode.BREAK:
                    {
                        PrintDisassembledLine(lastIP, op, "BREAK");
                        break;
                    }

                    default:
                        PrintDisassembledLine(lastIP, op, "? (" + op + ")");
                        break;
                }
            }
        }
    }
}
