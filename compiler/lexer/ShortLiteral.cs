namespace compiler.lexer
{
    public class ShortLiteral : NumericLiteral
    {
        public short Value
        {
            get;
        }

        internal ShortLiteral(SourceInterval interval, short value) : base(interval) => Value = value;

        public override string ToString() => Value.ToString();

        public override byte AsByte() => (byte) Value;

        public override short AsShort() => Value;

        public override int AsInt() => Value;

        public override long AsLong() => (long) Value;

        public override float AsFloat() => Value;

        public override double AsDouble() => Value;
    }
}
