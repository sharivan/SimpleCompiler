using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler
{
    public class LongLiteral : NumericLiteral
    {
        private long value;

        public long Value
        {
            get
            {
                return value;
            }
        }

        public LongLiteral(long value)
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
            return (short)value;
        }

        public override int AsInt()
        {
            return (int)value;
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
