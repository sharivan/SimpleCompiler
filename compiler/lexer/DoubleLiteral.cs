using System.Globalization;

namespace compiler.lexer
{
    public class DoubleLiteral : NumericLiteral
    {
        public double Value
        {
            get;
        }

        internal DoubleLiteral(SourceInterval interval, double value) : base(interval) => Value = value;

        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

        public override byte AsByte() => (byte) Value;

        public override short AsShort() => (short) Value;

        public override int AsInt() => (int) Value;

        public override long AsLong() => (long) Value;

        public override float AsFloat() => (float) Value;

        public override double AsDouble() => Value;
    }
}
