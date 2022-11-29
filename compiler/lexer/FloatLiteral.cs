using System.Globalization;

namespace compiler.lexer
{
    public class FloatLiteral : NumericLiteral
    {
        public float Value
        {
            get;
        }

        internal FloatLiteral(SourceInterval interval, float value) : base(interval) => Value = value;

        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

        public override byte AsByte() => (byte) Value;

        public override short AsShort() => (short) Value;

        public override int AsInt() => (int) Value;

        public override long AsLong() => (long) Value;

        public override float AsFloat() => Value;

        public override double AsDouble() => Value;
    }
}
