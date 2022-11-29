namespace compiler.lexer
{
    public class LongLiteral : NumericLiteral
    {
        public long Value
        {
            get;
        }

        internal LongLiteral(SourceInterval interval, long value) : base(interval) => Value = value;

        public override string ToString() => Value.ToString();

        public override byte AsByte() => (byte) Value;

        public override short AsShort() => (short) Value;

        public override int AsInt() => (int) Value;

        public override long AsLong() => Value;

        public override float AsFloat() => Value;

        public override double AsDouble() => Value;
    }
}
