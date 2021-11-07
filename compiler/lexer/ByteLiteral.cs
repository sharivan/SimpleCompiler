using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler.lexer
{
    public class ByteLiteral : NumericLiteral
    {
        private byte value;

        public byte Value => value;

        public ByteLiteral(SourceInterval interval, byte value) : base(interval)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public override byte AsByte()
        {
            return value;
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
