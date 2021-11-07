using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler.lexer
{
    public class DoubleLiteral : NumericLiteral
    {
        private double value;

        public double Value => value;

        public DoubleLiteral(SourceInterval interval, double value) : base(interval)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return value.ToString(CultureInfo.InvariantCulture);
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
            return (int) value;
        }

        public override long AsLong()
        {
            return (long) value;
        }

        public override float AsFloat()
        {
            return (float) value;
        }

        public override double AsDouble()
        {
            return value;
        }
    }
}
