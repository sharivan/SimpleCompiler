using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler
{
    public class Assembler
    {
		private MemoryStream output;
		private BinaryWriter writer;
		private List<Label> issuedLabels;
		private List<Label> bindedLabels;

		public long Position
		{
			get
			{
				return output.Position;
			}

			set
			{
				output.Position = value;
			}
		}

		public long CodeSize
		{
			get
			{
				return output.Position;
			}
		}

		public Assembler()
		{
			output = new MemoryStream();
			writer = new BinaryWriter(output);
			issuedLabels = new List<Label>();
			bindedLabels = new List<Label>();
		}

		public byte[] GetCode()
		{
			return output.ToArray();
		}

		public void Reset()
		{
			output.Position = 0;
		}

		public void BindLabel(Label label)
        {
			label.Bind(this, (int) output.Position);
			bindedLabels.Add(label);
		}

		public void EmitLabel(Label label)
		{
			label.AddReference(this, (int) output.Position);
			EmitLoadConst(label.BindedIP);
			issuedLabels.Add(label);
		}

		public void Emit(Assembler other)
		{
			long startPosition = output.Position;
			byte[] code = other.GetCode();
			output.Write(code, 0, code.Length);

			for (int i = 0; i < other.issuedLabels.Count; i++)
			{
				Label label = other.issuedLabels[i];

				for (int j = 0; j < label.references.Count; j++)
                {
					Tuple<Assembler, int> reference = label.references[j];
					int referenceIP = reference.Item2;
					label.references[j] = new Tuple<Assembler, int>(this, (int) (referenceIP + startPosition));
				}
				
				issuedLabels.Add(label);
			}

			for (int i = 0; i < other.bindedLabels.Count; i++)
			{
				Label label = other.bindedLabels[i];
				label.bindedAssembler = this;
				label.bindedIP += (int)startPosition;
				bindedLabels.Add(label);
			}

			other.issuedLabels.Clear();
			other.bindedLabels.Clear();
		}

		public void EmitNop()
		{
			writer.Write((byte)Opcode.NOP);
		}

		public void EmitLoadConst(bool flag)
		{
			writer.Write((byte)Opcode.LC8);
			writer.Write(flag);
		}

		public void EmitLoadConst(byte number)
		{
			writer.Write((byte)Opcode.LC8);
			writer.Write(number);
		}

		public void EmitLoadConst(char c)
		{
			writer.Write((byte)Opcode.LC16);
			writer.Write(c);
		}

		public void EmitLoadConst(short number)
		{
			writer.Write((byte)Opcode.LC16);
			writer.Write(number);
		}

		public void EmitLoadConst(int number)
		{
			writer.Write((byte)Opcode.LC32);
			writer.Write(number);
		}

		public void EmitLoadConst(long number)
		{
			writer.Write((byte)Opcode.LC64);
			writer.Write(number);
		}

		public void EmitLoadConst(float number)
		{
			writer.Write((byte)Opcode.LC32);
			writer.Write(BitConverter.ToInt32(BitConverter.GetBytes(number), 0));
		}

		public void EmitLoadConst(double number)
		{
			writer.Write((byte)Opcode.LC64);
			writer.Write(BitConverter.ToInt64(BitConverter.GetBytes(number), 0));
		}

		public void EmitLoadIP()
		{
			writer.Write((byte)Opcode.LIP);
		}

		public void EmitLoadSP()
		{
			writer.Write((byte)Opcode.LSP);
		}

		public void EmitLoadBP()
		{
			writer.Write((byte)Opcode.LBP);
		}

		public void EmitStoreIP()
		{
			writer.Write((byte)Opcode.SIP);
		}

		public void EmitStoreSP()
		{
			writer.Write((byte)Opcode.SSP);
		}

		public void EmitStoreBP()
		{
			writer.Write((byte)Opcode.SBP);
		}

		public void EmitLoadStack()
		{
			writer.Write((byte)Opcode.LS);
		}

		public void EmitLoadStack64()
		{
			writer.Write((byte)Opcode.LS64);
		}

		public void EmitStoreStack()
		{
			writer.Write((byte)Opcode.SS);
		}

		public void EmitStoreStack64()
		{
			writer.Write((byte)Opcode.SS64);
		}

		public void EmitAdd()
		{
			writer.Write((byte)Opcode.ADD);
		}

		public void EmitAdd64()
		{
			writer.Write((byte)Opcode.ADD64);
		}

		public void EmitSub()
		{
			writer.Write((byte)Opcode.SUB);
		}

		public void EmitSub64()
		{
			writer.Write((byte)Opcode.SUB64);
		}

		public void EmitMul()
		{
			writer.Write((byte)Opcode.MUL);
		}

		public void EmitMul64()
		{
			writer.Write((byte)Opcode.MUL64);
		}

		public void EmitDiv()
		{
			writer.Write((byte)Opcode.DIV);
		}

		public void EmitDiv64()
		{
			writer.Write((byte)Opcode.DIV64);
		}

		public void EmitMod()
		{
			writer.Write((byte)Opcode.MOD);
		}

		public void EmitMod64()
		{
			writer.Write((byte)Opcode.MOD64);
		}

		public void EmitNeg()
		{
			writer.Write((byte)Opcode.NEG);
		}

		public void EmitNeg64()
		{
			writer.Write((byte)Opcode.NEG64);
		}

		public void EmitAnd()
		{
			writer.Write((byte)Opcode.AND);
		}

		public void EmitAnd64()
		{
			writer.Write((byte)Opcode.AND64);
		}

		public void EmitOr()
		{
			writer.Write((byte)Opcode.OR);
		}

		public void EmitOr64()
		{
			writer.Write((byte)Opcode.OR64);
		}

		public void EmitXor()
		{
			writer.Write((byte)Opcode.XOR);
		}

		public void EmitXor64()
		{
			writer.Write((byte)Opcode.XOR64);
		}

		public void EmitNot()
		{
			writer.Write((byte)Opcode.NOT);
		}

		public void EmitNot64()
		{
			writer.Write((byte)Opcode.NOT64);
		}

		public void EmitShl()
		{
			writer.Write((byte)Opcode.SHL);
		}

		public void EmitShl64()
		{
			writer.Write((byte)Opcode.SHL64);
		}

		public void EmitShr()
		{
			writer.Write((byte)Opcode.SHR);
		}

		public void EmitShr64()
		{
			writer.Write((byte)Opcode.SHR64);
		}

		public void EmitUShr()
		{
			writer.Write((byte)Opcode.USHR);
		}

		public void EmitUShr64()
		{
			writer.Write((byte)Opcode.USHR64);
		}

		public void EmitFAdd()
		{
			writer.Write((byte)Opcode.FADD);
		}

		public void EmitFAdd64()
		{
			writer.Write((byte)Opcode.FADD64);
		}

		public void EmitFSub()
		{
			writer.Write((byte)Opcode.FSUB);
		}

		public void EmitFSub64()
		{
			writer.Write((byte)Opcode.FSUB64);
		}

		public void EmitFMul()
		{
			writer.Write((byte)Opcode.FMUL);
		}

		public void EmitFMul64()
		{
			writer.Write((byte)Opcode.FMUL64);
		}

		public void EmitFDiv()
		{
			writer.Write((byte)Opcode.FDIV);
		}

		public void EmitFDiv64()
		{
			writer.Write((byte)Opcode.FDIV64);
		}

		public void EmitFNeg()
		{
			writer.Write((byte)Opcode.FNEG);
		}

		public void EmitFNeg64()
		{
			writer.Write((byte)Opcode.FNEG64);
		}

		public void EmitInt32ToInt64()
		{
			writer.Write((byte)Opcode.I32I64);
		}

		public void EmitInt64ToInt32()
		{
			writer.Write((byte)Opcode.I64I32);
		}

		public void EmitInt32ToFloat32()
		{
			writer.Write((byte)Opcode.I32F32);
		}

		public void EmitInt32ToFloat64()
		{
			writer.Write((byte)Opcode.I32F64);
		}

		public void EmitInt64ToFloat64()
		{
			writer.Write((byte)Opcode.I64F64);
		}

		public void EmitFloat32ToInt32()
		{
			writer.Write((byte)Opcode.F32I32);
		}

		public void EmitFloat32ToInt64()
		{
			writer.Write((byte)Opcode.F32I64);
		}

		public void EmitFloat32ToFloat64()
		{
			writer.Write((byte)Opcode.F32F64);
		}

		public void EmitFloat64ToInt64()
		{
			writer.Write((byte)Opcode.F64I64);
		}

		public void EmitFloat64ToFloat32()
		{
			writer.Write((byte)Opcode.F64F32);
		}

		public void EmitCompareEquals()
		{
			writer.Write((byte)Opcode.CMPE);
		}

		public void EmitCompareNotEquals()
		{
			writer.Write((byte)Opcode.CMPNE);
		}

		public void EmitCompareGreater()
		{
			writer.Write((byte)Opcode.CMPG);
		}

		public void EmitCompareGreaterOrEquals()
		{
			writer.Write((byte)Opcode.CMPGE);
		}

		public void EmitCompareLess()
		{
			writer.Write((byte)Opcode.CMPL);
		}

		public void EmitCompareLessOrEquals()
		{
			writer.Write((byte)Opcode.CMPLE);
		}

		public void EmitCompareEquals64()
		{
			writer.Write((byte)Opcode.CMPE64);
		}

		public void EmitCompareNotEquals64()
		{
			writer.Write((byte)Opcode.CMPNE64);
		}

		public void EmitCompareGreater64()
		{
			writer.Write((byte)Opcode.CMPG64);
		}

		public void EmitCompareGreaterOrEquals64()
		{
			writer.Write((byte)Opcode.CMPGE64);
		}

		public void EmitCompareLess64()
		{
			writer.Write((byte)Opcode.CMPL64);
		}

		public void EmitCompareLessOrEquals64()
		{
			writer.Write((byte)Opcode.CMPLE64);
		}

		public void EmitFCompareEquals()
		{
			writer.Write((byte)Opcode.FCMPE);
		}

		public void EmitFCompareNotEquals()
		{
			writer.Write((byte)Opcode.FCMPNE);
		}

		public void EmitFCompareGreater()
		{
			writer.Write((byte)Opcode.FCMPG);
		}

		public void EmitFCompareGreaterOrEquals()
		{
			writer.Write((byte)Opcode.FCMPGE);
		}

		public void EmitFCompareLess()
		{
			writer.Write((byte)Opcode.FCMPL);
		}

		public void EmitFCompareLessOrEquals()
		{
			writer.Write((byte)Opcode.FCMPLE);
		}

		public void EmitFCompareEquals64()
		{
			writer.Write((byte)Opcode.FCMPE64);
		}

		public void EmitFCompareNotEquals64()
		{
			writer.Write((byte)Opcode.FCMPNE64);
		}

		public void EmitFCompareGreater64()
		{
			writer.Write((byte)Opcode.FCMPG64);
		}

		public void EmitFCompareGreaterOrEquals64()
		{
			writer.Write((byte)Opcode.FCMPGE64);
		}

		public void EmitFCompareLess64()
		{
			writer.Write((byte)Opcode.FCMPL64);
		}

		public void EmitFCompareLessOrEquals64()
		{
			writer.Write((byte)Opcode.FCMPLE64);
		}

		public void EmitJump()
		{
			writer.Write((byte)Opcode.JMP);
		}

		public void EmitJumpIfTrue()
		{
			writer.Write((byte)Opcode.JT);
		}

		public void EmitJumpIfFalse()
		{
			writer.Write((byte)Opcode.JF);
		}

		public void EmitPop()
		{
			writer.Write((byte)Opcode.POP);
		}

		public void EmitPop2()
		{
			writer.Write((byte)Opcode.POP2);
		}

		public void EmitPopN(int n)
		{
			writer.Write((byte)Opcode.POPN);
			writer.Write((byte)n);
		}

		public void EmitDup()
		{
			writer.Write((byte)Opcode.DUP);
		}

		public void EmitDup64()
		{
			writer.Write((byte)Opcode.DUP64);
		}

		public void EmitDupN(int n)
		{
			writer.Write((byte)Opcode.DUPN);
			writer.Write((byte)n);
		}

		public void EmitDup64N(int n)
		{
			writer.Write((byte)Opcode.DUP64N);
			writer.Write((byte)n);
		}

		public void EmitCall()
		{
			writer.Write((byte)Opcode.CALL);
		}

		public void EmitRet()
		{
			writer.Write((byte)Opcode.RET);
		}

		public void EmitScan()
		{
			writer.Write((byte)Opcode.SCAN);
		}

		public void EmitScan64()
		{
			writer.Write((byte)Opcode.SCAN64);
		}

		public void EmitFScan()
		{
			writer.Write((byte)Opcode.FSCAN);
		}

		public void EmitFScan64()
		{
			writer.Write((byte)Opcode.FSCAN64);
		}

		public void EmitPrint()
		{
			writer.Write((byte)Opcode.PRINT);
		}

		public void EmitPrint64()
		{
			writer.Write((byte)Opcode.PRINT64);
		}

		public void EmitFPrint()
		{
			writer.Write((byte)Opcode.FPRINT);
		}

		public void EmitFPrint64()
		{
			writer.Write((byte)Opcode.FPRINT64);
		}

		public void EmitHalt()
		{
			writer.Write((byte)Opcode.HALT);
		}
	}
}
