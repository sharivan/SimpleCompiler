using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler.lexer
{
    public class ShortLiteral : NumericLiteral
    {
        private short value;

        public short Value => value;

        internal ShortLiteral(SourceInterval interval, short value) : base(interval)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public override byte AsByte()
        {
            return (byte)value;
        }

        public override short AsShort()
        {
            return value;
        }

        public override int AsInt()
        {
            return value;
        }

        public override long AsLong()
        {
            return (long)value;
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
