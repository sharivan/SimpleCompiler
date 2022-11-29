using System;
using System.Collections.Generic;
using System.IO;

using vm;

namespace assembler
{
    public class Assembler
    {
        private readonly MemoryStream output;
        private readonly BinaryWriter writer;
        private readonly MemoryStream constantOut;
        private readonly BinaryWriter constantWritter;
        private readonly List<Label> issuedLabels;
        private readonly List<Label> bindedLabels;
        private readonly List<Tuple<string, int>> externalFunctions;

        public long Position
        {
            get => output.Position;

            set => output.Position = value;
        }

        public long CodeSize => output.Position;

        public long ConstantSize => constantOut.Position;

        public int ExternalFunctionCount => externalFunctions.Count;

        public Assembler()
        {
            output = new MemoryStream();
            writer = new BinaryWriter(output);
            constantOut = new MemoryStream();
            constantWritter = new BinaryWriter(constantOut);
            issuedLabels = new List<Label>();
            bindedLabels = new List<Label>();
            externalFunctions = new List<Tuple<string, int>>();
        }

        public void AddExternalFunctionNames(Tuple<string, int>[] entries) => externalFunctions.AddRange(entries);

        public Tuple<string, int> GetExternalFunction(int index) => externalFunctions[index];

        public void CopyCode(byte[] output) => CopyCode(output, 0, output.Length);

        public void CopyCode(byte[] output, int off, int len)
        {
            long count = this.output.Position;
            this.output.Position = 0;
            this.output.Read(output, off, len);
            this.output.Position = count;
        }

        public void CopyCode(Stream output) => this.output.WriteTo(output);

        public void CopyConstantBuffer(byte[] output) => CopyConstantBuffer(output, 0, output.Length);

        public void CopyConstantBuffer(byte[] output, int off, int len)
        {
            long count = constantOut.Position;
            constantOut.Position = 0;
            constantOut.Read(output, off, len);
            constantOut.Position = count;
        }

        public void CopyConstantBuffer(Stream output) => constantOut.WriteTo(output);

        public void ReserveConstantBuffer(int size)
        {
            constantOut.Position = 0;
            constantOut.Seek(size, SeekOrigin.Current);
        }

        public void WriteConstant(int offset, bool value)
        {
            constantOut.Position = offset;
            constantWritter.Write(value);
        }

        public void WriteConstant(int offset, byte value)
        {
            constantOut.Position = offset;
            constantWritter.Write(value);
        }

        public void WriteConstant(int offset, char value)
        {
            constantOut.Position = offset;
            constantWritter.Write(value);
        }

        public void WriteConstant(int offset, short value)
        {
            constantOut.Position = offset;
            constantWritter.Write(value);
        }

        public void WriteConstant(int offset, int value)
        {
            constantOut.Position = offset;
            constantWritter.Write(value);
        }

        public void WriteConstant(int offset, long value)
        {
            constantOut.Position = offset;
            constantWritter.Write(value);
        }

        public void WriteConstant(int offset, float value)
        {
            constantOut.Position = offset;
            constantWritter.Write(value);
        }

        public void WriteConstant(int offset, double value)
        {
            constantOut.Position = offset;
            constantWritter.Write(value);
        }

        public void WriteConstant(int offset, string value)
        {
            constantOut.Position = offset;
            byte[] bytes = new byte[value.Length * sizeof(char)];
            Buffer.BlockCopy(value.ToCharArray(), 0, bytes, 0, bytes.Length);
            constantWritter.Write(bytes);
            constantWritter.Write((short) 0);
        }

        public void WriteConstant(int offset, byte[] value)
        {
            constantOut.Position = offset;
            constantWritter.Write(value);
        }

        public void WriteConstant(int offset, byte[] value, int index, int count)
        {
            constantOut.Position = offset;
            constantWritter.Write(value, index, count);
        }

        public void Clear()
        {
            output.Position = 0;
            constantOut.Position = 0;

            issuedLabels.Clear();
            bindedLabels.Clear();
            externalFunctions.Clear();
        }

        public void EmitData(byte value) => writer.Write(value);

        public void EmitData(char value) => writer.Write(value);

        public void EmitData(short value) => writer.Write(value);

        public void EmitData(int value) => writer.Write(value);

        public void EmitData(long value) => writer.Write(value);

        public void EmitData(float value) => writer.Write(value);

        public void EmitData(double value) => writer.Write(value);

        public void EmitData(string value) => writer.Write(value);

        public void EmitData(byte[] buffer) => writer.Write(buffer);

        public void EmitData(byte[] buffer, int off, int len) => writer.Write(buffer, off, len);

        public void BindLabel(Label label)
        {
            label.Bind(this, (int) output.Position);
            bindedLabels.Add(label);
        }

        public void EmitLabel(Label label)
        {
            label.AddReference(this, (int) output.Position);
            writer.Write(label.BindedIP);
            issuedLabels.Add(label);
        }

        public void Emit(Assembler other)
        {
            long startPosition = output.Position;
            other.CopyCode(output);

            for (int i = 0; i < other.issuedLabels.Count; i++)
            {
                Label label = other.issuedLabels[i];

                for (int j = 0; j < label.references.Count; j++)
                {
                    Tuple<Assembler, int> reference = label.references[j];
                    Assembler referenceAssembler = reference.Item1;
                    if (referenceAssembler == other)
                    {
                        int referenceIP = reference.Item2;
                        label.references[j] = new Tuple<Assembler, int>(this, (int) (referenceIP + startPosition));
                    }
                }

                issuedLabels.Add(label);
            }

            for (int i = 0; i < other.bindedLabels.Count; i++)
            {
                Label label = other.bindedLabels[i];
                label.bindedAssembler = this;
                label.bindedIP += (int) startPosition;
                bindedLabels.Add(label);
            }
        }

        public void EmitNop() => writer.Write((byte) Opcode.NOP);

        public void EmitLoadConst(bool flag)
        {
            writer.Write((byte) Opcode.LC8);
            writer.Write((byte) (flag ? 1 : 0));
        }

        public void EmitLoadConst(byte number)
        {
            writer.Write((byte) Opcode.LC8);
            writer.Write(number);
        }

        public void EmitLoadConst(char c)
        {
            writer.Write((byte) Opcode.LC16);
            writer.Write((short) c);
        }

        public void EmitLoadConst(short number)
        {
            writer.Write((byte) Opcode.LC16);
            writer.Write(number);
        }

        public void EmitLoadConst(int number)
        {
            writer.Write((byte) Opcode.LC32);
            writer.Write(number);
        }

        public void EmitLoadConst(long number)
        {
            writer.Write((byte) Opcode.LC64);
            writer.Write(number);
        }

        public void EmitLoadConst(float number)
        {
            writer.Write((byte) Opcode.LC32);
            writer.Write(BitConverter.ToInt32(BitConverter.GetBytes(number), 0));
        }

        public void EmitLoadConst(double number)
        {
            writer.Write((byte) Opcode.LC64);
            writer.Write(BitConverter.ToInt64(BitConverter.GetBytes(number), 0));
        }

        public void EmitLoadConst(IntPtr ptr)
        {
            writer.Write((byte) Opcode.LC64);

            if (IntPtr.Size == sizeof(int))
                writer.Write((int) ptr);
            else
                writer.Write((long) ptr);
        }

        public void EmitLoadIP() => writer.Write((byte) Opcode.LIP);

        public void EmitLoadSP() => writer.Write((byte) Opcode.LSP);

        public void EmitLoadBP() => writer.Write((byte) Opcode.LBP);

        public void EmitStoreIP() => writer.Write((byte) Opcode.SIP);

        public void EmitStoreSP() => writer.Write((byte) Opcode.SSP);

        public void EmitStoreBP() => writer.Write((byte) Opcode.SBP);

        public void EmitAddSP(int offset)
        {
            writer.Write((byte) Opcode.ADDSP);
            writer.Write(offset);
        }

        public void EmitSubSP(int offset)
        {
            writer.Write((byte) Opcode.SUBSP);
            writer.Write(offset);
        }

        public void EmitLoadHostAddress() => writer.Write((byte) Opcode.LHA);

        public void EmitLoadGlobalHostAddress(int offset)
        {
            writer.Write((byte) Opcode.LGHA);
            writer.Write(offset);
        }

        public void EmitLoadLocalHostAddress(int offset)
        {
            writer.Write((byte) Opcode.LLHA);
            writer.Write(offset);
        }

        public void EmitLoadLocalResidentAddress(int offset)
        {
            writer.Write((byte) Opcode.LLRA);
            writer.Write(offset);
        }

        public void EmitResidentToHostAddress() => writer.Write((byte) Opcode.RHA);

        public void EmitHostToResidentAddress() => writer.Write((byte) Opcode.HRA);

        public void EmitLoadStack8() => writer.Write((byte) Opcode.LS8);

        public void EmitLoadStack16() => writer.Write((byte) Opcode.LS16);

        public void EmitLoadStack32() => writer.Write((byte) Opcode.LS32);

        public void EmitLoadStack64() => writer.Write((byte) Opcode.LS64);

        public void EmitLoadStackPtr() => writer.Write((byte) Opcode.LSPTR);

        public void EmitStoreStack8() => writer.Write((byte) Opcode.SS8);

        public void EmitStoreStack16() => writer.Write((byte) Opcode.SS16);

        public void EmitStoreStack32() => writer.Write((byte) Opcode.SS32);

        public void EmitStoreStack64() => writer.Write((byte) Opcode.SS64);

        public void EmitStoreStackPtr() => writer.Write((byte) Opcode.SSPTR);

        public void EmitLoadGlobal8(int offset)
        {
            writer.Write((byte) Opcode.LG8);
            writer.Write(offset);
        }

        public void EmitLoadGlobal16(int offset)
        {
            writer.Write((byte) Opcode.LG16);
            writer.Write(offset);
        }

        public void EmitLoadGlobal32(int offset)
        {
            writer.Write((byte) Opcode.LG32);
            writer.Write(offset);
        }

        public void EmitLoadGlobal64(int offset)
        {
            writer.Write((byte) Opcode.LG64);
            writer.Write(offset);
        }

        public void EmitLoadGlobalPtr(int offset)
        {
            writer.Write((byte) Opcode.LGPTR);
            writer.Write(offset);
        }

        public void EmitLoadLocal8(int offset)
        {
            writer.Write((byte) Opcode.LL8);
            writer.Write(offset);
        }

        public void EmitLoadLocal16(int offset)
        {
            writer.Write((byte) Opcode.LL16);
            writer.Write(offset);
        }

        public void EmitLoadLocal32(int offset)
        {
            writer.Write((byte) Opcode.LL32);
            writer.Write(offset);
        }

        public void EmitLoadLocal64(int offset)
        {
            writer.Write((byte) Opcode.LL64);
            writer.Write(offset);
        }

        public void EmitLoadLocalPtr(int offset)
        {
            writer.Write((byte) Opcode.LLPTR);
            writer.Write(offset);
        }

        public void EmitStoreGlobal8(int offset)
        {
            writer.Write((byte) Opcode.SG8);
            writer.Write(offset);
        }

        public void EmitStoreGlobal16(int offset)
        {
            writer.Write((byte) Opcode.SG16);
            writer.Write(offset);
        }

        public void EmitStoreGlobal32(int offset)
        {
            writer.Write((byte) Opcode.SG32);
            writer.Write(offset);
        }

        public void EmitStoreGlobal64(int offset)
        {
            writer.Write((byte) Opcode.SG64);
            writer.Write(offset);
        }

        public void EmitStoreGlobalPtr(int offset)
        {
            writer.Write((byte) Opcode.SGPTR);
            writer.Write(offset);
        }

        public void EmitStoreLocal8(int offset)
        {
            writer.Write((byte) Opcode.SL8);
            writer.Write(offset);
        }

        public void EmitStoreLocal16(int offset)
        {
            writer.Write((byte) Opcode.SL16);
            writer.Write(offset);
        }

        public void EmitStoreLocal32(int offset)
        {
            writer.Write((byte) Opcode.SL32);
            writer.Write(offset);
        }

        public void EmitStoreLocal64(int offset)
        {
            writer.Write((byte) Opcode.SL64);
            writer.Write(offset);
        }

        public void EmitStoreLocalPtr(int offset)
        {
            writer.Write((byte) Opcode.SLPTR);
            writer.Write(offset);
        }

        public void EmitLoadPointer8() => writer.Write((byte) Opcode.LPTR8);

        public void EmitLoadPointer16() => writer.Write((byte) Opcode.LPTR16);

        public void EmitLoadPointer32() => writer.Write((byte) Opcode.LPTR32);

        public void EmitLoadPointer64() => writer.Write((byte) Opcode.LPTR64);

        public void EmitLoadPointerPtr() => writer.Write((byte) Opcode.LPTRPTR);

        public void EmitStorePointer8() => writer.Write((byte) Opcode.SPTR8);

        public void EmitStorePointer16() => writer.Write((byte) Opcode.SPTR16);

        public void EmitStorePointer32() => writer.Write((byte) Opcode.SPTR32);

        public void EmitStorePointer64() => writer.Write((byte) Opcode.SPTR64);

        public void EmitStorePointerPtr() => writer.Write((byte) Opcode.SPTRPTR);

        public void EmitAdd() => writer.Write((byte) Opcode.ADD);

        public void EmitAdd64() => writer.Write((byte) Opcode.ADD64);

        public void EmitSub() => writer.Write((byte) Opcode.SUB);

        public void EmitSub64() => writer.Write((byte) Opcode.SUB64);

        public void EmitMul() => writer.Write((byte) Opcode.MUL);

        public void EmitMul64() => writer.Write((byte) Opcode.MUL64);

        public void EmitDiv() => writer.Write((byte) Opcode.DIV);

        public void EmitDiv64() => writer.Write((byte) Opcode.DIV64);

        public void EmitMod() => writer.Write((byte) Opcode.MOD);

        public void EmitMod64() => writer.Write((byte) Opcode.MOD64);

        public void EmitNeg() => writer.Write((byte) Opcode.NEG);

        public void EmitNeg64() => writer.Write((byte) Opcode.NEG64);

        public void EmitAnd() => writer.Write((byte) Opcode.AND);

        public void EmitAnd64() => writer.Write((byte) Opcode.AND64);

        public void EmitOr() => writer.Write((byte) Opcode.OR);

        public void EmitOr64() => writer.Write((byte) Opcode.OR64);

        public void EmitXor() => writer.Write((byte) Opcode.XOR);

        public void EmitXor64() => writer.Write((byte) Opcode.XOR64);

        public void EmitNot() => writer.Write((byte) Opcode.NOT);

        public void EmitNot64() => writer.Write((byte) Opcode.NOT64);

        public void EmitShl() => writer.Write((byte) Opcode.SHL);

        public void EmitShl64() => writer.Write((byte) Opcode.SHL64);

        public void EmitShr() => writer.Write((byte) Opcode.SHR);

        public void EmitShr64() => writer.Write((byte) Opcode.SHR64);

        public void EmitUShr() => writer.Write((byte) Opcode.USHR);

        public void EmitUShr64() => writer.Write((byte) Opcode.USHR64);

        public void EmitFAdd() => writer.Write((byte) Opcode.FADD);

        public void EmitFAdd64() => writer.Write((byte) Opcode.FADD64);

        public void EmitFSub() => writer.Write((byte) Opcode.FSUB);

        public void EmitFSub64() => writer.Write((byte) Opcode.FSUB64);

        public void EmitFMul() => writer.Write((byte) Opcode.FMUL);

        public void EmitFMul64() => writer.Write((byte) Opcode.FMUL64);

        public void EmitFDiv() => writer.Write((byte) Opcode.FDIV);

        public void EmitFDiv64() => writer.Write((byte) Opcode.FDIV64);

        public void EmitFNeg() => writer.Write((byte) Opcode.FNEG);

        public void EmitFNeg64() => writer.Write((byte) Opcode.FNEG64);

        public void EmitPtrAdd() => writer.Write((byte) Opcode.PTRADD);

        public void EmitPtrAdd64() => writer.Write((byte) Opcode.PTRADD64);

        public void EmitPtrSub() => writer.Write((byte) Opcode.PTRSUB);

        public void EmitPtrSub64() => writer.Write((byte) Opcode.PTRSUB64);

        public void EmitInt32ToInt64() => writer.Write((byte) Opcode.I32I64);

        public void EmitInt64ToInt32() => writer.Write((byte) Opcode.I64I32);

        public void EmitInt32ToFloat32() => writer.Write((byte) Opcode.I32F32);

        public void EmitInt32ToFloat64() => writer.Write((byte) Opcode.I32F64);

        public void EmitInt64ToFloat64() => writer.Write((byte) Opcode.I64F64);

        public void EmitFloat32ToInt32() => writer.Write((byte) Opcode.F32I32);

        public void EmitFloat32ToInt64() => writer.Write((byte) Opcode.F32I64);

        public void EmitFloat32ToFloat64() => writer.Write((byte) Opcode.F32F64);

        public void EmitFloat64ToInt64() => writer.Write((byte) Opcode.F64I64);

        public void EmitFloat64ToFloat32() => writer.Write((byte) Opcode.F64F32);

        public void EmitInt32ToPointer() => writer.Write((byte) Opcode.I32PTR);

        public void EmitInt64ToPointer() => writer.Write((byte) Opcode.I64PTR);

        public void EmitPointerToInt32() => writer.Write((byte) Opcode.PTRI32);

        public void EmitPointerToInt64() => writer.Write((byte) Opcode.PTRI64);

        public void EmitCompareEquals() => writer.Write((byte) Opcode.CMPE);

        public void EmitCompareNotEquals() => writer.Write((byte) Opcode.CMPNE);

        public void EmitCompareGreater() => writer.Write((byte) Opcode.CMPG);

        public void EmitCompareGreaterOrEquals() => writer.Write((byte) Opcode.CMPGE);

        public void EmitCompareLess() => writer.Write((byte) Opcode.CMPL);

        public void EmitCompareLessOrEquals() => writer.Write((byte) Opcode.CMPLE);

        public void EmitCompareEquals64() => writer.Write((byte) Opcode.CMPE64);

        public void EmitCompareNotEquals64() => writer.Write((byte) Opcode.CMPNE64);

        public void EmitCompareGreater64() => writer.Write((byte) Opcode.CMPG64);

        public void EmitCompareGreaterOrEquals64() => writer.Write((byte) Opcode.CMPGE64);

        public void EmitCompareLess64() => writer.Write((byte) Opcode.CMPL64);

        public void EmitCompareLessOrEquals64() => writer.Write((byte) Opcode.CMPLE64);

        public void EmitFCompareEquals() => writer.Write((byte) Opcode.FCMPE);

        public void EmitFCompareNotEquals() => writer.Write((byte) Opcode.FCMPNE);

        public void EmitFCompareGreater() => writer.Write((byte) Opcode.FCMPG);

        public void EmitFCompareGreaterOrEquals() => writer.Write((byte) Opcode.FCMPGE);

        public void EmitFCompareLess() => writer.Write((byte) Opcode.FCMPL);

        public void EmitFCompareLessOrEquals() => writer.Write((byte) Opcode.FCMPLE);

        public void EmitFCompareEquals64() => writer.Write((byte) Opcode.FCMPE64);

        public void EmitFCompareNotEquals64() => writer.Write((byte) Opcode.FCMPNE64);

        public void EmitFCompareGreater64() => writer.Write((byte) Opcode.FCMPG64);

        public void EmitFCompareGreaterOrEquals64() => writer.Write((byte) Opcode.FCMPGE64);

        public void EmitFCompareLess64() => writer.Write((byte) Opcode.FCMPL64);

        public void EmitFCompareLessOrEquals64() => writer.Write((byte) Opcode.FCMPLE64);

        public void EmitComparePointerEquals() => writer.Write((byte) Opcode.CMPEPTR);

        public void EmitComparePointerNotEquals() => writer.Write((byte) Opcode.CMPNEPTR);

        public void EmitComparePointerGreater() => writer.Write((byte) Opcode.CMPGPTR);

        public void EmitComparePointerGreaterOrEquals() => writer.Write((byte) Opcode.CMPGEPTR);

        public void EmitComparePointerLess() => writer.Write((byte) Opcode.CMPLPTR);

        public void EmitComparePointerLessOrEquals() => writer.Write((byte) Opcode.CMPLEPTR);

        public void EmitJump(int offset)
        {
            writer.Write((byte) Opcode.JMP);
            writer.Write(offset);
        }

        public void EmitJump(Label label)
        {
            writer.Write((byte) Opcode.JMP);
            EmitLabel(label);
        }

        public void EmitJumpIfTrue(int offset)
        {
            writer.Write((byte) Opcode.JT);
            writer.Write(offset);
        }

        public void EmitJumpIfTrue(Label label)
        {
            writer.Write((byte) Opcode.JT);
            EmitLabel(label);
        }

        public void EmitJumpIfFalse(int offset)
        {
            writer.Write((byte) Opcode.JF);
            writer.Write(offset);
        }

        public void EmitJumpIfFalse(Label label)
        {
            writer.Write((byte) Opcode.JF);
            EmitLabel(label);
        }

        public void EmitPop() => writer.Write((byte) Opcode.POP);

        public void EmitPop2() => writer.Write((byte) Opcode.POP2);

        public void EmitPopN(int n)
        {
            writer.Write((byte) Opcode.POPN);
            writer.Write((byte) n);
        }

        public void EmitDup() => writer.Write((byte) Opcode.DUP);

        public void EmitDup64() => writer.Write((byte) Opcode.DUP64);

        public void EmitDupN(int n)
        {
            writer.Write((byte) Opcode.DUPN);
            writer.Write((byte) n);
        }

        public void EmitDup64N(int n)
        {
            writer.Write((byte) Opcode.DUP64N);
            writer.Write((byte) n);
        }

        public void EmitCall(int offset)
        {
            writer.Write((byte) Opcode.CALL);
            writer.Write(offset);
        }

        public void EmitCall(Label label)
        {
            writer.Write((byte) Opcode.CALL);
            EmitLabel(label);
        }

        public void EmitIndirectCall() => writer.Write((byte) Opcode.ICALL);

        public void EmitExternCall(int index)
        {
            writer.Write((byte) Opcode.ECALL);
            writer.Write(index);
        }

        public void EmitRet() => writer.Write((byte) Opcode.RET);

        public void EmitRetN(int count)
        {
            writer.Write((byte) Opcode.RETN);
            writer.Write(count);
        }

        public void EmitScanB() => writer.Write((byte) Opcode.SCANB);

        public void EmitScan8() => writer.Write((byte) Opcode.SCAN8);

        public void EmitScanC() => writer.Write((byte) Opcode.SCANC);

        public void EmitScan16() => writer.Write((byte) Opcode.SCAN16);

        public void EmitScan32() => writer.Write((byte) Opcode.SCAN32);

        public void EmitScan64() => writer.Write((byte) Opcode.SCAN64);

        public void EmitFScan() => writer.Write((byte) Opcode.FSCAN);

        public void EmitFScan64() => writer.Write((byte) Opcode.FSCAN64);

        public void EmitScanString() => writer.Write((byte) Opcode.SCANSTR);

        public void EmitPrintB() => writer.Write((byte) Opcode.PRINTB);

        public void EmitPrintC() => writer.Write((byte) Opcode.PRINTC);

        public void EmitPrint32() => writer.Write((byte) Opcode.PRINT32);

        public void EmitPrint64() => writer.Write((byte) Opcode.PRINT64);

        public void EmitFPrint() => writer.Write((byte) Opcode.FPRINT);

        public void EmitFPrint64() => writer.Write((byte) Opcode.FPRINT64);

        public void EmitPrintString() => writer.Write((byte) Opcode.PRINTSTR);

        public void EmitHalt() => writer.Write((byte) Opcode.HALT);
    }
}
