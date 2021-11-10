using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler.lexer
{
    public class IntLiteral : NumericLiteral
    {
        private int value;

        public int Value => value;

        internal IntLiteral(SourceInterval interval, int value) : base(interval)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public override byte AsByte()
        {
            return (byte) value;
        }

        public override short AsShort()
        {
            return (short) value;
        }

        public override int AsInt()
        {
            return value;
        }

        public override long AsLong()
        {
            return value;
        }

        public override float AsFloat()
        {
            return value;
        }

        public override double AsDouble()
        {
            return value;
        }
    }
}
