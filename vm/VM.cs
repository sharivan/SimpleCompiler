using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using assembler;
using units;

namespace vm
{
    public class VM
    {
        public delegate void ExternalFunctionHandler(VM vm);

        public const int DEFAULT_STACK_SIZE = 32798; // tamanho da pilha bytes

        public delegate string ConsoleRead();
        public delegate void ConsolePrint(string message);

        private byte[] code;
        private byte[] stack;

        // registradores
        private int ip; // ponteiro de instrução
        private int sp; // ponteiro de pilha
        private int bp; // ponteiro de base

        private List<ExternalFunctionHandler> externalFunctions;
        private Dictionary<string, int> externalFunctionMap;

        public event ConsoleRead OnConsoleRead;
        public event ConsolePrint OnConsolePrint;

        public int IP
        {
            get
            {
                return ip;
            }

            set
            {
                ip = value;
            }
        }

        public int SP
        {
            get
            {
                return sp;
            }

            set
            {
                sp = value;
            }
        }

        public int BP
        {
            get
            {
                return bp;
            }

            set
            {
                bp = value;
            }
        }

        public int CodeSize
        {
            get
            {
                return code.Length;
            }
        }

        public int StackSize
        {
            get
            {
                return stack.Length;
            }
        }

        public VM()
        {
            code = null;
            stack = new byte[DEFAULT_STACK_SIZE];

            externalFunctions = new List<ExternalFunctionHandler>();
            externalFunctionMap = new Dictionary<string, int>();
        }

        public void Initialize(Assembler assembler, int stackSize = DEFAULT_STACK_SIZE)
        {
            code = assembler.GetCode();
            sp = 0;

            Array.Resize(ref stack, stackSize + (int) assembler.ConstantSize);

            if (assembler.ConstantSize > 0)
            {
                byte[] constantBuffer = assembler.GetConstantBuffer();
                Push(constantBuffer);
            }

            externalFunctions.Clear();
            externalFunctionMap.Clear();

            for (int i = 0; i < assembler.ExternalFunctionCount; i++)
            {
                string functionName = assembler.GetExternalFunctionName(i);
                AddExternalFunction(functionName);
            }

            foreach (var kv in UnitySystem.FUNCTIONS)
                BindExternalFunction(kv.Key, kv.Value);
        }

        public void AddExternalFunction(string functionName)
        {
            if (externalFunctionMap.ContainsKey(functionName))
                throw new Exception("External function " + functionName + " already added.");

            externalFunctions.Add(null);
            externalFunctionMap.Add(functionName, externalFunctions.Count - 1);
        }

        public void BindExternalFunction(string functionName, ExternalFunctionHandler function)
        {
            if (externalFunctionMap.ContainsKey(functionName))
            {              
                int index = externalFunctionMap[functionName];
                externalFunctions[index] = function;
            }
            else
            {
                externalFunctions.Add(function);
                externalFunctionMap.Add(functionName, externalFunctions.Count - 1);
            }
        }

        public byte ReadCodeByte(ref int ip)
        {
            return code[ip++];
        }

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

        public IntPtr ReadCodePtr(ref int ip)
        {
            if (IntPtr.Size == sizeof(int))
                return (IntPtr) ReadCodeInt(ref ip);

            return (IntPtr) ReadCodeLong(ref ip);
        }

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

        public byte ReadStackByte(int addr)
        {
            int result = stack[addr] & MASK0;
            return (byte) result;
        }

        public short ReadStackShort(int addr)
        {
            int result = stack[addr++] & MASK0;
            result |= (stack[addr] << 8) & MASK1;
            return (short) result;
        }

        public int ReadStackInt(int addr)
        {
            int result = stack[addr++] & MASK0;
            result |= (stack[addr++] << 8) & MASK1;
            result |= (stack[addr++] << 16) & MASK2;
            result |= (stack[addr] << 24) & MASK3;
            return result;
        }

        public long ReadStackLong(int addr)
        {
            long result = stack[addr++] & MASK0;
            result |= ((long) stack[addr++] << 8) & MASK1;
            result |= ((long) stack[addr++] << 16) & MASK2;
            result |= ((long) stack[addr++] << 24) & MASK3;
            result |= ((long) stack[addr++] << 32) & MASK4;
            result |= ((long) stack[addr++] << 40) & MASK5;
            result |= ((long) stack[addr++] << 48) & MASK6;
            result |= ((long) stack[addr] << 56) & MASK7;
            return result;
        }

        public IntPtr ReadStackPtr(int addr)
        {
            if (IntPtr.Size == sizeof(int))
                return (IntPtr) ReadStackInt(addr);

            return (IntPtr) ReadStackLong(addr);
        }

        public float ReadStackFloat(int addr)
        {
            int value = ReadStackInt(addr);
            return BitConverter.ToSingle(BitConverter.GetBytes(value), 0);
        }

        public double ReadStackDouble(int addr)
        {
            long value = ReadStackLong(addr);
            return BitConverter.ToDouble(BitConverter.GetBytes(value), 0);
        }

        public string ReadStackString(int addr)
        {
            string result = "";
            while (true)
            {
                char c = (char) ReadStackShort(addr);
                addr += sizeof(short);
                if (c == '\0')
                    break;

                result += c;
            }

            return result;
        }

        public void WriteStack(int addr, byte value)
        {
            stack[addr] = value;
        }

        public void WriteStack(int addr, short value)
        {
            stack[addr++] = (byte) value;
            stack[addr] = (byte) (value >> 8);
        }

        public void WriteStack(int addr, int value)
        {
            stack[addr++] = (byte) value;
            stack[addr++] = (byte) (value >> 8);
            stack[addr++] = (byte) (value >> 16);
            stack[addr] = (byte) (value >> 24);
        }

        public void WriteStack(int addr, long value)
        {
            stack[addr++] = (byte) value;
            stack[addr++] = (byte) (value >> 8);
            stack[addr++] = (byte) (value >> 16);
            stack[addr++] = (byte) (value >> 24);
            stack[addr++] = (byte) (value >> 32);
            stack[addr++] = (byte) (value >> 40);
            stack[addr++] = (byte) (value >> 48);
            stack[addr] = (byte) (value >> 56);
        }

        public void WriteStack(int addr, IntPtr ptr)
        {
            if (IntPtr.Size == sizeof(int))
                WriteStack(addr, (int) ptr);
            else
                WriteStack(addr, (long) ptr);
        }

        public void WriteStack(int addr, float value)
        {
            WriteStack(addr, BitConverter.ToInt32(BitConverter.GetBytes(value), 0));
        }

        public void WriteStack(int addr, double value)
        {
            WriteStack(addr, BitConverter.ToInt64(BitConverter.GetBytes(value), 0));
        }

        public void WriteStack(int addr, string value)
        {
            byte[] bytes = new byte[value.Length * sizeof(char)];
            WriteStack(addr, value);
            WriteStack(addr + bytes.Length, (short) 0);
        }

        public void WriteStack(int addr, byte[] buf)
        {
            WriteStack(addr, buf, 0, buf.Length);
        }

        public void WriteStack(int addr, byte[] buf, int off, int len)
        {
            Array.Copy(buf, off, stack, addr, len);
        }

        public void MoveBlock(int srcAddr, int dstAddr, int len)
        {
            Array.Copy(stack, srcAddr, stack, dstAddr, len);
        }

        public void LoadBlock(int srcAddr, int len)
        {
            MoveBlock(srcAddr, sp, len);
            sp += len;
        }

        public void StoreBlock(int dstAddr, int len)
        {
            sp -= len;
            MoveBlock(sp, dstAddr, len);            
        }

        public int ReadStackTop()
        {
            return ReadStackInt(SP - sizeof(int));
        }

        public long ReadStackTop64()
        {
            return ReadStackLong(SP - sizeof(long));
        }

        public IntPtr ReadStackTopPtr()
        {
            return ReadStackPtr(SP - IntPtr.Size);
        }

        public float ReadStackTopFloat()
        {
            return ReadStackFloat(SP - sizeof(float));
        }

        public double ReadStackTopDouble()
        {
            return ReadStackDouble(SP - sizeof(double));
        }

        public void Push(int value)
        {
            WriteStack(sp, value);
            sp += sizeof(int);
        }

        public void Push(long value)
        {
            WriteStack(sp, value);
            sp += sizeof(long);
        }

        public void Push(IntPtr value)
        {
            WriteStack(sp, value);
            sp += IntPtr.Size;
        }

        public void Push(float value)
        {
            Push(BitConverter.ToInt32(BitConverter.GetBytes(value), 0));
        }

        public void Push(double value)
        {
            Push(BitConverter.ToInt64(BitConverter.GetBytes(value), 0));
        }

        public void Push(byte[] buf)
        {
            Push(buf, 0, buf.Length);
        }

        public void Push(byte[] buf, int off, int len)
        {
            WriteStack(sp, buf, off, len);
            sp += len;
        }

        public int Pop()
        {
            sp -= sizeof(int);
            int result = ReadStackInt(sp);
            return result;
        }

        public long PopLong()
        {
            sp -= sizeof(long);
            long result = ReadStackLong(sp);
            return result;
        }

        public IntPtr PopPtr()
        {
            sp -= IntPtr.Size;
            IntPtr result = ReadStackPtr(sp);
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

        public IntPtr ResidentToHostAddr(int addr)
        {
            return Marshal.UnsafeAddrOfPinnedArrayElement(stack, addr);
        }

        public int HostToResidentAddr(IntPtr addr)
        {
            return (int) ((long) addr - (long) Marshal.UnsafeAddrOfPinnedArrayElement(stack, 0));
        }

        private int GetParamAbsoluteOffset(int paramsSize, int index, int paramSize)
        {
            return bp - 2 * sizeof(int) - paramsSize + (index + 1) * paramSize;
        }

        public IntPtr LoadParamAddr(int paramsSize, int index, int paramSize)
        {
            return Marshal.UnsafeAddrOfPinnedArrayElement(stack, GetParamAbsoluteOffset(paramsSize, index, paramSize));
        }

        public int LoadParam(int paramsSize, int index)
        {
            return ReadStackInt(GetParamAbsoluteOffset(paramsSize, index, sizeof(int)));
        }

        public long LoadParamLong(int paramsSize, int index)
        {
            return ReadStackLong(GetParamAbsoluteOffset(paramsSize, index, sizeof(long)));
        }

        public IntPtr LoadParamPtr(int paramsSize, int index)
        {
            return ReadStackPtr(GetParamAbsoluteOffset(paramsSize, index, IntPtr.Size));
        }

        public float LoadParamFloat(int paramsSize, int index)
        {
            return ReadStackFloat(GetParamAbsoluteOffset(paramsSize, index, sizeof(float)));
        }

        public double LoadParamDouble(int paramsSize, int index)
        {
            return ReadStackDouble(GetParamAbsoluteOffset(paramsSize, index, sizeof(double)));
        }

        public void SetParam(int paramsSize, int index, int value)
        {
            WriteStack(GetParamAbsoluteOffset(paramsSize, index, sizeof(int)), value);
        }

        public void SetParam(int paramsSize, int index, long value)
        {
            WriteStack(GetParamAbsoluteOffset(paramsSize, index, sizeof(long)), value);
        }

        public void SetParam(int paramsSize, int index, IntPtr value)
        {
            WriteStack(GetParamAbsoluteOffset(paramsSize, index, IntPtr.Size), value);
        }

        public void SetParam(int paramsSize, int index, float value)
        {
            WriteStack(GetParamAbsoluteOffset(paramsSize, index, sizeof(float)), value);
        }

        public void SetParam(int paramsSize, int index, double value)
        {
            WriteStack(GetParamAbsoluteOffset(paramsSize, index, sizeof(double)), value);
        }

        public void Run()
        {
            ip = 0;
            bp = sp;

            while (ip < code.Length)
            {
                int lastIP = ip;
                int op = ReadCodeByte(ref ip) & 0xff;
                Opcode Opcode = (Opcode) op;

                switch (Opcode)
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
                        Push(sp);
                        break;
                    }

                    case Opcode.LBP:
                    {
                        Push(bp);
                        break;
                    }

                    case Opcode.SIP:
                    {
                        ip = Pop();
                        break;
                    }

                    case Opcode.SSP:
                    {
                        sp = Pop();
                        break;
                    }

                    case Opcode.SBP:
                    {
                        bp = Pop();
                        break;
                    }

                    case Opcode.ADDSP:
                    {
                        int offset = ReadCodeInt(ref ip);
                        sp += offset;
                        break;
                    }

                    case Opcode.SUBSP:
                    {
                        int offset = ReadCodeInt(ref ip);
                        sp -= offset;
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
                        Push(ResidentToHostAddr(bp + offset));
                        break;
                    }

                    case Opcode.LLRA:
                    {
                        int offset = ReadCodeInt(ref ip);
                        Push(bp + offset);
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
                        byte value = ReadStackByte(bp + offset);
                        Push(value);
                        break;
                    }

                    case Opcode.LL16:
                    {
                        int offset = ReadCodeInt(ref ip);
                        short value = ReadStackShort(bp + offset);
                        Push(value);
                        break;
                    }

                    case Opcode.LL32:
                    {
                        int offset = ReadCodeInt(ref ip);
                        int value = ReadStackInt(bp + offset);
                        Push(value);
                        break;
                    }

                    case Opcode.LL64:
                    {
                        int offset = ReadCodeInt(ref ip);
                        long value = ReadStackLong(bp + offset);
                        Push(value);
                        break;
                    }

                    case Opcode.LLPTR:
                    {
                        int offset = ReadCodeInt(ref ip);
                        IntPtr value = ReadStackPtr(bp + offset);
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
                        WriteStack(bp + offset, (byte) value);
                        break;
                    }

                    case Opcode.SL16:
                    {
                        int offset = ReadCodeInt(ref ip);
                        int value = Pop();
                        WriteStack(bp + offset, (short) value);
                        break;
                    }

                    case Opcode.SL32:
                    {
                        int offset = ReadCodeInt(ref ip);
                        int value = Pop();
                        WriteStack(bp + offset, value);
                        break;
                    }

                    case Opcode.SL64:
                    {
                        int offset = ReadCodeInt(ref ip);
                        long value = PopLong();
                        WriteStack(bp + offset, value);
                        break;
                    }

                    case Opcode.SLPTR:
                    {
                        int offset = ReadCodeInt(ref ip);
                        IntPtr value = PopPtr();
                        WriteStack(bp + offset, value);
                        break;
                    }

                    case Opcode.LPTR8:
                    {
                        IntPtr addr = PopPtr();

                        unsafe
                        {
                            byte result = *(byte*) addr;
                            Push(result);
                        }

                        break;
                    }

                    case Opcode.LPTR16:
                    {
                        IntPtr addr = PopPtr();

                        unsafe
                        {
                            short result = *(short*) addr;
                            Push(result);
                        }

                        break;
                    }

                    case Opcode.LPTR32:
                    {
                        IntPtr addr = PopPtr();

                        unsafe
                        {
                            int result = *(int*) addr;
                            Push(result);
                        }

                        break;
                    }

                    case Opcode.LPTR64:
                    {
                        IntPtr addr = PopPtr();

                        unsafe
                        {
                            long result = *(long*) addr;
                            Push(result);
                        }

                        break;
                    }

                    case Opcode.LPTRPTR:
                    {
                        IntPtr addr = PopPtr();

                        unsafe
                        {
                            IntPtr result = *(IntPtr *) addr;
                            Push(result);
                        }

                        break;
                    }

                    case Opcode.SPTR8:
                    {
                        int value = Pop();
                        IntPtr addr = PopPtr();

                        unsafe
                        {
                            *((byte *) addr) = (byte) value;
                        }

                        break;
                    }

                    case Opcode.SPTR16:
                    {
                        int value = Pop();
                        IntPtr addr = PopPtr();

                        unsafe
                        {
                            *((short*) addr) = (short) value;
                        }

                        break;
                    }

                    case Opcode.SPTR32:
                    {
                        int value = Pop();
                        IntPtr addr = PopPtr();

                        unsafe
                        {
                            *((int*) addr) = value;
                        }

                        break;
                    }

                    case Opcode.SPTR64:
                    {
                        long value = PopLong();
                        IntPtr addr = PopPtr();

                        unsafe
                        {
                            *((long*) addr) = value;
                        }

                        break;
                    }

                    case Opcode.SPTRPTR:
                    {
                        IntPtr value = PopPtr();
                        IntPtr addr = PopPtr();

                        unsafe
                        {
                            *((IntPtr*) addr) = value;
                        }

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
                        IntPtr result = (IntPtr) operand;
                        Push(result);
                        break;
                    }

                    case Opcode.I64PTR:
                    {
                        long operand = PopLong();
                        IntPtr result = (IntPtr) operand;
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

                        bool result;
                        if (IntPtr.Size == sizeof(int))
                            result = (int) operand1 > (int) operand2;
                        else
                            result = (long) operand1 > (long) operand2;

                        Push(result ? 1 : 0);
                        break;
                    }

                    case Opcode.CMPGEPTR:
                    {
                        IntPtr operand2 = PopPtr();
                        IntPtr operand1 = PopPtr();

                        bool result;
                        if (IntPtr.Size == sizeof(int))
                            result = (int) operand1 >= (int) operand2;
                        else
                            result = (long) operand1 >= (long) operand2;

                        Push(result ? 1 : 0);
                        break;
                    }

                    case Opcode.CMPLPTR:
                    {
                        IntPtr operand2 = PopPtr();
                        IntPtr operand1 = PopPtr();

                        bool result;
                        if (IntPtr.Size == sizeof(int))
                            result = (int) operand1 < (int) operand2;
                        else
                            result = (long) operand1 < (long) operand2;

                        Push(result ? 1 : 0);
                        break;
                    }

                    case Opcode.CMPLEPTR:
                    {
                        IntPtr operand2 = PopPtr();
                        IntPtr operand1 = PopPtr();

                        bool result;
                        if (IntPtr.Size == sizeof(int))
                            result = (int) operand1 <= (int) operand2;
                        else
                            result = (long) operand1 <= (long) operand2;

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
                        Push(bp);
                        bp = sp;
                        ip = lastIP + offset;
                        break;
                    }

                    case Opcode.ICALL:
                    {
                        int offset = Pop();
                        Push(ip);
                        Push(bp);
                        bp = sp;
                        ip = lastIP + offset;
                        break;
                    }

                    case Opcode.ECALL:
                    {
                        int index = Pop();
                        Push(ip);
                        Push(bp);
                        bp = sp;
                        externalFunctions[index](this);
                        break;
                    }

                    case Opcode.RET:
                    {
                        bp = Pop();
                        ip = Pop();
                        break;
                    }

                    case Opcode.RETN:
                    {
                        int count = ReadCodeInt(ref ip);
                        bp = Pop();
                        ip = Pop();
                        sp -= count;
                        break;
                    }

                    case Opcode.SCANB:
                    {
                        int addr = Pop();
                        string str = OnConsoleRead();
                        try
                        {
                            if (str == "verdade" || str == "1")
                                WriteStack(addr, 1);
                            else if (str == "falso" || str == "0")
                                WriteStack(addr, 0);
                            else
                            {
                                bool value = bool.Parse(str);
                                WriteStack(addr, value ? 1 : 0);
                            }
                        }
                        catch (Exception)
                        {
                            WriteStack(addr, (byte) 0);
                        }

                        break;
                    }

                    case Opcode.SCAN8:
                    {
                        int addr = Pop();
                        string str = OnConsoleRead();
                        try
                        {
                            int value = int.Parse(str);
                            WriteStack(addr, (byte) value);
                        }
                        catch (Exception)
                        {
                            WriteStack(addr, (byte) 0);
                        }

                        break;
                    }

                    case Opcode.SCANC:
                    {
                        int addr = Pop();
                        string str = OnConsoleRead();
                        if (str.Length == 0)
                            WriteStack(addr, (short) 0);
                        else
                            WriteStack(addr, (short) str[0]);

                        break;
                    }

                    case Opcode.SCAN16:
                    {
                        int addr = Pop();
                        string str = OnConsoleRead();
                        try
                        {
                            int value = int.Parse(str);
                            WriteStack(addr, (short) value);
                        }
                        catch (Exception)
                        {
                            WriteStack(addr, (short) 0);
                        }

                        break;
                    }

                    case Opcode.SCAN32:
                    {
                        int addr = Pop();
                        string str = OnConsoleRead();
                        try
                        {
                            int value = int.Parse(str);
                            WriteStack(addr, value);
                        }
                        catch (Exception)
                        {
                            WriteStack(addr, 0);
                        }

                        break;
                    }

                    case Opcode.SCAN64:
                    {
                        int addr = Pop();
                        string str = OnConsoleRead();
                        try
                        {
                            long value = long.Parse(str);
                            WriteStack(addr, value);
                        }
                        catch (Exception)
                        {
                            WriteStack(addr, 0L);
                        }

                        break;
                    }

                    case Opcode.FSCAN:
                    {
                        int addr = Pop();
                        string str = OnConsoleRead();
                        try
                        {
                            float value = float.Parse(str, CultureInfo.InvariantCulture);
                            WriteStack(addr, value);
                        }
                        catch (Exception)
                        {
                            WriteStack(addr, 0F);
                        }

                        break;
                    }

                    case Opcode.FSCAN64:
                    {
                        int addr = Pop();
                        string str = OnConsoleRead();
                        try
                        {
                            double value = double.Parse(str, CultureInfo.InvariantCulture);
                            WriteStack(addr, value);
                        }
                        catch (Exception)
                        {
                            WriteStack(addr, 0.0);
                        }

                        break;
                    }

                    case Opcode.SCANSTR:
                    {
                        int addr = Pop();
                        string value = OnConsoleRead();
                        WriteStack(addr, value);
                        break;
                    }

                    case Opcode.PRINT32:
                    {
                        int value = Pop();
                        Print(value.ToString());
                        break;
                    }

                    case Opcode.PRINT64:
                    {
                        long value = PopLong();
                        Print(value.ToString());
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
                        int addr = Pop();
                        string value = addr != 0 ? ReadStackString(addr) : "nulo";
                        Print(value.ToString(CultureInfo.InvariantCulture));
                        break;
                    }

                    case Opcode.HALT:
                    {
                        return;
                    }

                    default:
                        throw new Exception("Illegal Opcode " + op + " at IP " + lastIP);
                }
            }
        }

        private static readonly char[] HEX_DIGITS = { 'a', 'b', 'c', 'd', 'e', 'f' };

        private string ToHex(int n)
        {
            string result = "";
            return result;
        }

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

        private string FormatHex(int n, int digits)
        {
            string result = ToHex(n);
            int diff = digits - result.Length;
            if (diff > 0)
                for (int i = 0; i < diff; i++)
                    result = "0" + result;

            return result;
        }

        private void PrintLn()
        {
            Print("\n");
        }

        private void PrintLn(string message)
        {
            Print(message + '\n');
        }

        private void Print(string message)
        {
            OnConsolePrint(message);
        }

        private void PrintLine(int ip, int op, string s)
        {
            PrintLn(Format(ip, 8) + "  " + Format(op, 3) + "  " + s);
        }

        public void Print()
        {
            int ip = 0;
            while (ip < code.Length)
            {
                int lastIP = ip;
                int op = ReadCodeByte(ref ip) & 0xff;
                Opcode Opcode = (Opcode) op;

                switch (Opcode)
                {
                    case Opcode.NOP:
                        PrintLine(lastIP, op, "NOP");
                        break;

                    case Opcode.LC8:
                    {
                        int value = ReadCodeByte(ref ip);
                        PrintLine(lastIP, op, "LC8 " + value);
                        break;
                    }

                    case Opcode.LC16:
                    {
                        int value = ReadCodeShort(ref ip);
                        PrintLine(lastIP, op, "LC16 " + value);
                        break;
                    }

                    case Opcode.LC32:
                    {
                        int value = ReadCodeInt(ref ip);
                        PrintLine(lastIP, op, "LC32 " + value + " //" + BitConverter.ToSingle(BitConverter.GetBytes(value), 0).ToString(CultureInfo.InvariantCulture));
                        break;
                    }

                    case Opcode.LC64:
                    {
                        long value = ReadCodeLong(ref ip);
                        PrintLine(lastIP, op, "LC64 " + value + " //" + BitConverter.ToDouble(BitConverter.GetBytes(value), 0).ToString(CultureInfo.InvariantCulture));
                        break;
                    }

                    case Opcode.LCPTR:
                    {
                        long value = ReadCodeLong(ref ip);
                        PrintLine(lastIP, op, "LCPTR " + value + " //" + BitConverter.ToDouble(BitConverter.GetBytes(value), 0).ToString(CultureInfo.InvariantCulture));
                        break;
                    }

                    case Opcode.LIP:
                    {
                        PrintLine(lastIP, op, "LIP");
                        break;
                    }

                    case Opcode.LSP:
                    {
                        PrintLine(lastIP, op, "LSP");
                        break;
                    }

                    case Opcode.LBP:
                    {
                        PrintLine(lastIP, op, "LBP");
                        break;
                    }

                    case Opcode.SIP:
                    {
                        PrintLine(lastIP, op, "SIP");
                        break;
                    }

                    case Opcode.SSP:
                    {
                        PrintLine(lastIP, op, "SSP");
                        break;
                    }

                    case Opcode.SBP:
                    {
                        PrintLine(lastIP, op, "SBP");
                        break;
                    }

                    case Opcode.ADDSP:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintLine(lastIP, op, "ADDSP " + offset);
                        break;
                    }

                    case Opcode.SUBSP:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintLine(lastIP, op, "SUBSP " + offset);
                        break;
                    }

                    case Opcode.LLHA:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintLine(lastIP, op, "LLHA " + offset + (offset < 0 ? " // param" : ""));
                        break;
                    }

                    case Opcode.LLRA:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintLine(lastIP, op, "LLRA " + offset + (offset < 0 ? " // param" : ""));
                        break;
                    }

                    case Opcode.RHA:
                    {
                        PrintLine(lastIP, op, "RHA");
                        break;
                    }

                    case Opcode.HRA:
                    {
                        PrintLine(lastIP, op, "RHA");
                        break;
                    }

                    case Opcode.LS8:
                    {
                        PrintLine(lastIP, op, "LS8");
                        break;
                    }

                    case Opcode.LS16:
                    {
                        PrintLine(lastIP, op, "LS16");
                        break;
                    }

                    case Opcode.LS32:
                    {
                        PrintLine(lastIP, op, "LS32");
                        break;
                    }

                    case Opcode.LS64:
                    {
                        PrintLine(lastIP, op, "LS64");
                        break;
                    }

                    case Opcode.LSPTR:
                    {
                        PrintLine(lastIP, op, "LSPTR");
                        break;
                    }

                    case Opcode.SS8:
                    {
                        PrintLine(lastIP, op, "SS8");
                        break;
                    }

                    case Opcode.SS16:
                    {
                        PrintLine(lastIP, op, "SS16");
                        break;
                    }

                    case Opcode.SS32:
                    {
                        PrintLine(lastIP, op, "SS32");
                        break;
                    }

                    case Opcode.SS64:
                    {
                        PrintLine(lastIP, op, "SS64");
                        break;
                    }

                    case Opcode.SSPTR:
                    {
                        PrintLine(lastIP, op, "SSPTR");
                        break;
                    }

                    case Opcode.LG8:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintLine(lastIP, op, "LG8 " + offset);
                        break;
                    }

                    case Opcode.LG16:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintLine(lastIP, op, "LG16 " + offset);
                        break;
                    }

                    case Opcode.LG32:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintLine(lastIP, op, "LG32 " + offset);
                        break;
                    }

                    case Opcode.LG64:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintLine(lastIP, op, "LG64 " + offset);
                        break;
                    }

                    case Opcode.LGPTR:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintLine(lastIP, op, "LGPTR " + offset);
                        break;
                    }

                    case Opcode.LL8:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintLine(lastIP, op, "LL8 " + offset + (offset < 0 ? " // param" : ""));
                        break;
                    }

                    case Opcode.LL16:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintLine(lastIP, op, "LL16 " + offset + (offset < 0 ? " // param" : ""));
                        break;
                    }

                    case Opcode.LL32:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintLine(lastIP, op, "LL32 " + offset + (offset < 0 ? " // param" : ""));
                        break;
                    }

                    case Opcode.LL64:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintLine(lastIP, op, "LL64 " + offset + (offset < 0 ? " // param" : ""));
                        break;
                    }

                    case Opcode.LLPTR:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintLine(lastIP, op, "LLPTR " + offset + (offset < 0 ? " // param" : ""));
                        break;
                    }

                    case Opcode.SG8:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintLine(lastIP, op, "SG8 " + offset);
                        break;
                    }

                    case Opcode.SG16:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintLine(lastIP, op, "SG16 " + offset);
                        break;
                    }

                    case Opcode.SG32:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintLine(lastIP, op, "SG32 " + offset);
                        break;
                    }

                    case Opcode.SG64:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintLine(lastIP, op, "SG64 " + offset);
                        break;
                    }

                    case Opcode.SGPTR:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintLine(lastIP, op, "SGPTR " + offset);
                        break;
                    }

                    case Opcode.SL8:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintLine(lastIP, op, "SL8 " + offset + (offset < 0 ? " // param" : ""));
                        break;
                    }

                    case Opcode.SL16:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintLine(lastIP, op, "SL16 " + offset + (offset < 0 ? " // param" : ""));
                        break;
                    }

                    case Opcode.SL32:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintLine(lastIP, op, "SL32 " + offset + (offset < 0 ? " // param" : ""));
                        break;
                    }

                    case Opcode.SL64:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintLine(lastIP, op, "SL64 " + offset + (offset < 0 ? " // param" : ""));
                        break;
                    }

                    case Opcode.SLPTR:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintLine(lastIP, op, "SLPTR " + offset + (offset < 0 ? " // param" : ""));
                        break;
                    }

                    case Opcode.LPTR8:
                    {
                        PrintLine(lastIP, op, "LPTR8");
                        break;
                    }

                    case Opcode.LPTR16:
                    {
                        PrintLine(lastIP, op, "LPTR16");
                        break;
                    }

                    case Opcode.LPTR32:
                    {
                        PrintLine(lastIP, op, "LPTR32");
                        break;
                    }

                    case Opcode.LPTR64:
                    {
                        PrintLine(lastIP, op, "LPTR64");
                        break;
                    }

                    case Opcode.LPTRPTR:
                    {
                        PrintLine(lastIP, op, "LPTRPTR");
                        break;
                    }

                    case Opcode.SPTR8:
                    {
                        PrintLine(lastIP, op, "SPTR8");
                        break;
                    }

                    case Opcode.SPTR16:
                    {
                        PrintLine(lastIP, op, "SPTR16");
                        break;
                    }

                    case Opcode.SPTR32:
                    {
                        PrintLine(lastIP, op, "SPTR32");
                        break;
                    }

                    case Opcode.SPTR64:
                    {
                        PrintLine(lastIP, op, "SPTR64");
                        break;
                    }

                    case Opcode.SPTRPTR:
                    {
                        PrintLine(lastIP, op, "SPTRPTR");
                        break;
                    }

                    case Opcode.ADD:
                    {
                        PrintLine(lastIP, op, "ADD");
                        break;
                    }

                    case Opcode.ADD64:
                    {
                        PrintLine(lastIP, op, "ADD64");
                        break;
                    }

                    case Opcode.SUB:
                    {
                        PrintLine(lastIP, op, "SUB");
                        break;
                    }

                    case Opcode.SUB64:
                    {
                        PrintLine(lastIP, op, "SUB64");
                        break;
                    }

                    case Opcode.MUL:
                    {
                        PrintLine(lastIP, op, "MUL");
                        break;
                    }

                    case Opcode.MUL64:
                    {
                        PrintLine(lastIP, op, "MUL64");
                        break;
                    }

                    case Opcode.DIV:
                    {
                        PrintLine(lastIP, op, "DIV");
                        break;
                    }

                    case Opcode.DIV64:
                    {
                        PrintLine(lastIP, op, "DIV64");
                        break;
                    }

                    case Opcode.MOD:
                    {
                        PrintLine(lastIP, op, "MOD");
                        break;
                    }

                    case Opcode.MOD64:
                    {
                        PrintLine(lastIP, op, "MOD64");
                        break;
                    }

                    case Opcode.NEG:
                    {
                        PrintLine(lastIP, op, "NEG");
                        break;
                    }

                    case Opcode.NEG64:
                    {
                        PrintLine(lastIP, op, "NEG64");
                        break;
                    }

                    case Opcode.AND:
                    {
                        PrintLine(lastIP, op, "AND");
                        break;
                    }

                    case Opcode.AND64:
                    {
                        PrintLine(lastIP, op, "AND64");
                        break;
                    }

                    case Opcode.OR:
                    {
                        PrintLine(lastIP, op, "OR");
                        break;
                    }

                    case Opcode.OR64:
                    {
                        PrintLine(lastIP, op, "OR64");
                        break;
                    }

                    case Opcode.XOR:
                    {
                        PrintLine(lastIP, op, "XOR");
                        break;
                    }

                    case Opcode.XOR64:
                    {
                        PrintLine(lastIP, op, "XOR64");
                        break;
                    }

                    case Opcode.NOT:
                    {
                        PrintLine(lastIP, op, "NOT");
                        break;
                    }

                    case Opcode.NOT64:
                    {
                        PrintLine(lastIP, op, "NOT64");
                        break;
                    }

                    case Opcode.SHL:
                    {
                        PrintLine(lastIP, op, "SHL");
                        break;
                    }

                    case Opcode.SHL64:
                    {
                        PrintLine(lastIP, op, "SHL64");
                        break;
                    }

                    case Opcode.SHR:
                    {
                        PrintLine(lastIP, op, "SHR");
                        break;
                    }

                    case Opcode.SHR64:
                    {
                        PrintLine(lastIP, op, "SHR64");
                        break;
                    }

                    case Opcode.USHR:
                    {
                        PrintLine(lastIP, op, "USHR");
                        break;
                    }

                    case Opcode.USHR64:
                    {
                        PrintLine(lastIP, op, "USHR64");
                        break;
                    }

                    case Opcode.FADD:
                    {
                        PrintLine(lastIP, op, "FADD");
                        break;
                    }

                    case Opcode.FADD64:
                    {
                        PrintLine(lastIP, op, "FADD64");
                        break;
                    }

                    case Opcode.FSUB:
                    {
                        PrintLine(lastIP, op, "FSUB");
                        break;
                    }

                    case Opcode.FSUB64:
                    {
                        PrintLine(lastIP, op, "FSUB64");
                        break;
                    }

                    case Opcode.FMUL:
                    {
                        PrintLine(lastIP, op, "FMUL");
                        break;
                    }

                    case Opcode.FMUL64:
                    {
                        PrintLine(lastIP, op, "FMUL64");
                        break;
                    }

                    case Opcode.FDIV:
                    {
                        PrintLine(lastIP, op, "FDIV");
                        break;
                    }

                    case Opcode.FDIV64:
                    {
                        PrintLine(lastIP, op, "FDIV64");
                        break;
                    }

                    case Opcode.FNEG:
                    {
                        PrintLine(lastIP, op, "FNEG");
                        break;
                    }

                    case Opcode.FNEG64:
                    {
                        PrintLine(lastIP, op, "FNEG64");
                        break;
                    }

                    case Opcode.PTRADD:
                    {
                        PrintLine(lastIP, op, "PTRADD");
                        break;
                    }

                    case Opcode.PTRADD64:
                    {
                        PrintLine(lastIP, op, "PTRADD64");
                        break;
                    }

                    case Opcode.I32I64:
                    {
                        PrintLine(lastIP, op, "I32I64");
                        break;
                    }

                    case Opcode.I64I32:
                    {
                        PrintLine(lastIP, op, "I64I32");
                        break;
                    }

                    case Opcode.I32F32:
                    {
                        PrintLine(lastIP, op, "I32F32");
                        break;
                    }

                    case Opcode.I32F64:
                    {
                        PrintLine(lastIP, op, "I32F64");
                        break;
                    }

                    case Opcode.I64F64:
                    {
                        PrintLine(lastIP, op, "I64F64");
                        break;
                    }

                    case Opcode.F32F64:
                    {
                        PrintLine(lastIP, op, "F32F64");
                        break;
                    }

                    case Opcode.F32I32:
                    {
                        PrintLine(lastIP, op, "F32I32");
                        break;
                    }

                    case Opcode.F32I64:
                    {
                        PrintLine(lastIP, op, "F32I64");
                        break;
                    }

                    case Opcode.F64I64:
                    {
                        PrintLine(lastIP, op, "F64I64");
                        break;
                    }

                    case Opcode.F64F32:
                    {
                        PrintLine(lastIP, op, "F64F32");
                        break;
                    }

                    case Opcode.I32PTR:
                    {
                        PrintLine(lastIP, op, "I32PTR");
                        break;
                    }

                    case Opcode.I64PTR:
                    {
                        PrintLine(lastIP, op, "I64PTR");
                        break;
                    }

                    case Opcode.PTRI32:
                    {
                        PrintLine(lastIP, op, "PTRI32");
                        break;
                    }

                    case Opcode.PTRI64:
                    {
                        PrintLine(lastIP, op, "PTRI64");
                        break;
                    }

                    case Opcode.CMPE:
                    {
                        PrintLine(lastIP, op, "CMPE");
                        break;
                    }

                    case Opcode.CMPNE:
                    {
                        PrintLine(lastIP, op, "CMPNE");
                        break;
                    }

                    case Opcode.CMPG:
                    {
                        PrintLine(lastIP, op, "CMPG");
                        break;
                    }

                    case Opcode.CMPGE:
                    {
                        PrintLine(lastIP, op, "CMPGE");
                        break;
                    }

                    case Opcode.CMPL:
                    {
                        PrintLine(lastIP, op, "CMPL");
                        break;
                    }

                    case Opcode.CMPLE:
                    {
                        PrintLine(lastIP, op, "CMPLE");
                        break;
                    }

                    case Opcode.CMPE64:
                    {
                        PrintLine(lastIP, op, "CMPE64");
                        break;
                    }

                    case Opcode.CMPNE64:
                    {
                        PrintLine(lastIP, op, "CMPNE64");
                        break;
                    }

                    case Opcode.CMPG64:
                    {
                        PrintLine(lastIP, op, "CMPG64");
                        break;
                    }

                    case Opcode.CMPGE64:
                    {
                        PrintLine(lastIP, op, "CMPGE64");
                        break;
                    }

                    case Opcode.CMPL64:
                    {
                        PrintLine(lastIP, op, "CMPL64");
                        break;
                    }

                    case Opcode.CMPLE64:
                    {
                        PrintLine(lastIP, op, "CMPLE64");
                        break;
                    }

                    case Opcode.FCMPE:
                    {
                        PrintLine(lastIP, op, "FCMPE");
                        break;
                    }

                    case Opcode.FCMPNE:
                    {
                        PrintLine(lastIP, op, "FCMPNE");
                        break;
                    }

                    case Opcode.FCMPG:
                    {
                        PrintLine(lastIP, op, "FCMPG");
                        break;
                    }

                    case Opcode.FCMPGE:
                    {
                        PrintLine(lastIP, op, "FCMPGE");
                        break;
                    }

                    case Opcode.FCMPL:
                    {
                        PrintLine(lastIP, op, "FCMPL");
                        break;
                    }

                    case Opcode.FCMPLE:
                    {
                        PrintLine(lastIP, op, "FCMPLE");
                        break;
                    }

                    case Opcode.FCMPE64:
                    {
                        PrintLine(lastIP, op, "FCMPE64");
                        break;
                    }

                    case Opcode.FCMPNE64:
                    {
                        PrintLine(lastIP, op, "FCMPNE64");
                        break;
                    }

                    case Opcode.FCMPG64:
                    {
                        PrintLine(lastIP, op, "FCMPG64");
                        break;
                    }

                    case Opcode.FCMPGE64:
                    {
                        PrintLine(lastIP, op, "FCMPGE64");
                        break;
                    }

                    case Opcode.FCMPL64:
                    {
                        PrintLine(lastIP, op, "FCMPL64");
                        break;
                    }

                    case Opcode.FCMPLE64:
                    {
                        PrintLine(lastIP, op, "FCMPLE64");
                        break;
                    }

                    case Opcode.CMPEPTR:
                    {
                        PrintLine(lastIP, op, "CMPEPTR");
                        break;
                    }

                    case Opcode.CMPNEPTR:
                    {
                        PrintLine(lastIP, op, "CMPNEPTR");
                        break;
                    }

                    case Opcode.CMPGPTR:
                    {
                        PrintLine(lastIP, op, "CMPGPTR");
                        break;
                    }

                    case Opcode.CMPGEPTR:
                    {
                        PrintLine(lastIP, op, "CMPGEPTR");
                        break;
                    }

                    case Opcode.CMPLPTR:
                    {
                        PrintLine(lastIP, op, "CMPLPTR");
                        break;
                    }

                    case Opcode.CMPLEPTR:
                    {
                        PrintLine(lastIP, op, "CMPLEPTR");
                        break;
                    }

                    case Opcode.JMP:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintLine(lastIP, op, "JMP " + Format(lastIP + offset, 8) + " //" + (offset > 0 ? "+" : "") + offset);
                        break;
                    }

                    case Opcode.JT:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintLine(lastIP, op, "JT " + Format(lastIP + offset, 8) + " //" + (offset > 0 ? "+" : "") + offset);
                        break;
                    }

                    case Opcode.JF:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintLine(lastIP, op, "JF " + Format(lastIP + offset, 8) + " //" + (offset > 0 ? "+" : "") + offset);
                        break;
                    }

                    case Opcode.POP:
                    {
                        PrintLine(lastIP, op, "POP");
                        break;
                    }

                    case Opcode.POP2:
                    {
                        PrintLine(lastIP, op, "POP2");
                        break;
                    }

                    case Opcode.POPN:
                    {
                        byte n = ReadCodeByte(ref ip);
                        PrintLine(lastIP, op, "POPN " + n);
                        break;
                    }

                    case Opcode.DUP:
                    {
                        PrintLine(lastIP, op, "DUP");
                        break;
                    }

                    case Opcode.DUP64:
                    {
                        PrintLine(lastIP, op, "DUP64");
                        break;
                    }

                    case Opcode.DUPN:
                    {
                        byte n = ReadCodeByte(ref ip);
                        PrintLine(lastIP, op, "DUPN " + n);
                        break;
                    }

                    case Opcode.DUP64N:
                    {
                        byte n = ReadCodeByte(ref ip);
                        PrintLine(lastIP, op, "DUP64N " + n);
                        break;
                    }

                    case Opcode.CALL:
                    {
                        int offset = ReadCodeInt(ref ip);
                        PrintLine(lastIP, op, "CALL " + Format(lastIP + offset, 8) + " //" + (offset > 0 ? "+" : "") + offset);
                        break;
                    }

                    case Opcode.ICALL:
                    {
                        PrintLine(lastIP, op, "ICALL");
                        break;
                    }

                    case Opcode.ECALL:
                    {
                        PrintLine(lastIP, op, "ECALL");
                        break;
                    }

                    case Opcode.RET:
                    {
                        PrintLine(lastIP, op, "RET");
                        PrintLn();
                        break;
                    }

                    case Opcode.RETN:
                    {
                        int count = ReadCodeInt(ref ip);
                        PrintLine(lastIP, op, "RETN " + count);
                        PrintLn();
                        break;
                    }

                    case Opcode.SCAN8:
                    {
                        PrintLine(lastIP, op, "SCAN8");
                        break;
                    }

                    case Opcode.SCAN16:
                    {
                        PrintLine(lastIP, op, "SCAN16");
                        break;
                    }

                    case Opcode.SCAN32:
                    {
                        PrintLine(lastIP, op, "SCAN32");
                        break;
                    }

                    case Opcode.SCAN64:
                    {
                        PrintLine(lastIP, op, "SCAN64");
                        break;
                    }

                    case Opcode.FSCAN:
                    {
                        PrintLine(lastIP, op, "FSCAN");
                        break;
                    }

                    case Opcode.FSCAN64:
                    {
                        PrintLine(lastIP, op, "FSCAN64");
                        break;
                    }

                    case Opcode.SCANSTR:
                    {
                        PrintLine(lastIP, op, "SCANSTR");
                        break;
                    }

                    case Opcode.PRINT32:
                    {
                        PrintLine(lastIP, op, "PRINT32");
                        break;
                    }

                    case Opcode.PRINT64:
                    {
                        PrintLine(lastIP, op, "PRINT64");
                        break;
                    }

                    case Opcode.FPRINT:
                    {
                        PrintLine(lastIP, op, "FPRINT");
                        break;
                    }

                    case Opcode.FPRINT64:
                    {
                        PrintLine(lastIP, op, "FPRINT64");
                        break;
                    }

                    case Opcode.PRINTSTR:
                    {
                        PrintLine(lastIP, op, "PRINTSTR");
                        break;
                    }

                    case Opcode.HALT:
                    {
                        PrintLine(lastIP, op, "HALT");
                        PrintLn();
                        break;
                    }

                    default:
                        PrintLine(lastIP, op, "? (" + op + ")");
                        break;
                }
            }
        }
    }
}
