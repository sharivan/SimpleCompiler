using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler
{
    public class VM
    {
		public const int DEFAULT_STACK_SIZE = 4096; // tamanho da pilha em dwords (bytes * 4)

		public delegate string ConsoleRead();
		public delegate void ConsolePrint(string message);

		private byte[] code;
		private int[] stack;

		// registradores
		private int ip; // ponteiro de instrução
		private int sp; // ponteiro de pilha
		private int bp; // ponteiro de base

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
		}

		public void Initialize(Assembler assembler, int stackSize = DEFAULT_STACK_SIZE)
		{
			code = assembler.GetCode();
			stack = new int[stackSize];
		}

		private byte ReadCodeByte(ref int ip)
		{
			return code[ip++];
		}

		private const int MASK0 = 0xff;
		private const int MASK1 = MASK0 << 8;
		private const int MASK2 = MASK0 << 16;
		private const int MASK3 = MASK0 << 24;
		private const long MASK4 = (long) MASK0 << 32;
		private const long MASK5 = (long)MASK0 << 40;
		private const long MASK6 = (long)MASK0 << 48;
		private const long MASK7 = (long)MASK0 << 56;

		private short ReadCodeShort(ref int ip)
		{
			int result = code[ip++] & MASK0;
			result |= (code[ip++] << 8) & MASK1;
			return (short)result;
		}

		private int ReadCodeInt(ref int ip)
		{
			int result = code[ip++] & MASK0;
			result |= (code[ip++] << 8) & MASK1;
			result |= (code[ip++] << 16) & MASK2;
			result |= (int) ((code[ip++] << 24) & MASK3);
			return result;
		}

		private long ReadCodeLong(ref int ip)
		{
			long result = code[ip++] & MASK0;
			result |= ((long)code[ip++] << 8) & MASK1;
			result |= ((long)code[ip++] << 16) & MASK2;
			result |= ((long)code[ip++] << 24) & MASK3;
			result |= ((long)code[ip++] << 32) & MASK4;
			result |= ((long)code[ip++] << 40) & MASK5;
			result |= ((long)code[ip++] << 48) & MASK6;
			result |= ((long)code[ip++] << 56) & MASK7;
			return result;
		}

		private float ReadCodeFloat(ref int ip)
		{
			int value = ReadCodeInt(ref ip);
			return BitConverter.ToSingle(BitConverter.GetBytes(value), 0);
		}

		private double ReadCodeDouble(ref int ip)
		{
			long value = ReadCodeLong(ref ip);
			return BitConverter.ToDouble(BitConverter.GetBytes(value), 0);
		}

		public int ReadStack(int addr)
		{
			return stack[addr];
		}

		const int MASKA = ~0;
		const long MASKB = (long)MASKA << 32;

		public long ReadStack64(int addr)
		{
			long result = stack[addr] & MASKA;
			result |= ((long)stack[addr + 1] << 32) & MASKB;
			return result;
		}

		public float ReadStackFloat(int addr)
		{
			int value = ReadStack(addr);
			return BitConverter.ToSingle(BitConverter.GetBytes(value), 0);
		}

		public double ReadStackDouble(int addr)
		{
			long value = ReadStack64(addr);
			return BitConverter.ToDouble(BitConverter.GetBytes(value), 0);
		}

		public void WriteStack(int addr, int value)
		{
			stack[addr] = value;
		}

		public void WriteStack(int addr, long value)
		{
			stack[addr] = (int)(value & 0xffffffff);
			stack[addr + 1] = (int)((value >> 32) & 0xffffffff);
		}

		public void WriteStack(int addr, float value)
		{
			WriteStack(addr, BitConverter.ToInt32(BitConverter.GetBytes(value), 0));
		}

		public void WriteStack(int addr, double value)
		{
			WriteStack(addr, BitConverter.ToInt64(BitConverter.GetBytes(value), 0));
		}

		public int ReadStackTop()
		{
			return ReadStack(SP - 1);
		}

		public long ReadStackTop64()
		{
			return ReadStack64(SP - 2);
		}

		public float ReadStackTopFloat()
		{
			return ReadStackFloat(SP - 1);
		}

		public double ReadStackTopDouble()
		{
			return ReadStackDouble(SP - 2);
		}

		public void Push(int value)
		{
			stack[sp++] = value;
		}

		public void Push(long value)
		{
			stack[sp++] = (int)(value & 0xffffffff);
			stack[sp++] = (int)((value >> 32) & 0xffffffff);
		}

		public void Push(float value)
		{
			Push(BitConverter.ToInt32(BitConverter.GetBytes(value), 0));
		}

		public void Push(double value)
		{
			Push(BitConverter.ToInt64(BitConverter.GetBytes(value), 0));
		}

		public int Pop()
		{
			return stack[--sp];
		}

		public long PopLong()
		{
			sp -= 2;
			long result = stack[sp] & MASKA;
			result |= ((long)stack[sp + 1] << 32) & MASKB;
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

		public void Run()
		{
			ip = 0;
			sp = 0;
			bp = 0;

			while (ip < code.Length)
			{
				int lastIP = ip;
				int op = ReadCodeByte(ref ip) & 0xff;
				Opcode Opcode = (Opcode)op;

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

					case Opcode.LS:
						{
							int addr = Pop();
							int value = ReadStack(addr);
							Push(value);
							break;
						}

					case Opcode.LS64:
						{
							int addr = Pop();
							long value = ReadStack64(addr);
							Push(value);
							break;
						}

					case Opcode.SS:
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
							int result = (int)operand;
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
							int result = (int)operand;
							Push(result);
							break;
						}

					case Opcode.F32I64:
						{
							float operand = PopFloat();
							long result = (long)operand;
							Push(result);
							break;
						}

					case Opcode.F64I64:
						{
							double operand = PopDouble();
							long result = (long)operand;
							Push(result);
							break;
						}

					case Opcode.F64F32:
						{
							double operand = PopDouble();
							float result = (float)operand;
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
							bool result = operand1 > operand2;
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

					case Opcode.JMP:
						{
							int delta = Pop();
							ip = lastIP + delta;
							break;
						}

					case Opcode.JT:
						{
							int delta = Pop();
							int value = Pop() & 1;

							if (value == 1)
								ip = lastIP + delta;

							break;
						}

					case Opcode.JF:
						{
							int delta = Pop();
							int value = Pop() & 1;

							if (value == 0)
								ip = lastIP + delta;

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
							int delta = Pop();
							Push(ip);
							Push(bp);
							bp = sp;
							ip = lastIP + delta;
							break;
						}

					case Opcode.RET:
						{
							bp = Pop();
							ip = Pop();
							break;
						}

					case Opcode.SCAN:
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
								Push(0);
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
								Push(0L);
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
								Push(0F);
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
								Push(0.0);
							}

							break;
						}

					case Opcode.PRINT:
						{
							int value = Pop();
							PrintLn(value.ToString());
							break;
						}

					case Opcode.PRINT64:
						{
							long value = PopLong();
							PrintLn(value.ToString());
							break;
						}

					case Opcode.FPRINT:
						{
							float value = PopFloat();
							PrintLn(value.ToString(CultureInfo.InvariantCulture));
							break;
						}

					case Opcode.FPRINT64:
						{
							double value = PopDouble();
							PrintLn(value.ToString(CultureInfo.InvariantCulture));
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
			string result = n.ToString(); ;
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
				Opcode Opcode = (Opcode)op;

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

					case Opcode.LS:
						{
							PrintLine(lastIP, op, "LS");
							break;
						}

					case Opcode.LS64:
						{
							PrintLine(lastIP, op, "LS64");
							break;
						}

					case Opcode.SS:
						{
							PrintLine(lastIP, op, "SS");
							break;
						}

					case Opcode.SS64:
						{
							PrintLine(lastIP, op, "SS64");
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

					case Opcode.JMP:
						{
							PrintLine(lastIP, op, "JMP");
							break;
						}

					case Opcode.JT:
						{
							PrintLine(lastIP, op, "JT");
							break;
						}

					case Opcode.JF:
						{
							PrintLine(lastIP, op, "JF");
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
							PrintLine(lastIP, op, "CALL");
							break;
						}

					case Opcode.RET:
						{
							PrintLine(lastIP, op, "RET");
							break;
						}

					case Opcode.SCAN:
						{
							PrintLine(lastIP, op, "SCAN");
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

					case Opcode.PRINT:
						{
							PrintLine(lastIP, op, "PRINT");
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

					case Opcode.HALT:
						{
							PrintLine(lastIP, op, "HALT");
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
