namespace compiler.lexer
{
    public class IntLiteral : NumericLiteral
    {
        public int Value
        {
            get;
        }

        internal IntLiteral(SourceInterval interval, int value) : base(interval) => Value = value;

        public override string ToString() => Value.ToString();

        public override byte AsByte() => (byte) Value;

        public override short AsShort() => (short) Value;

        public override int AsInt() => Value;

        public override long AsLong() => Value;

        public override float AsFloat() => Value;

        public override double AsDouble() => Value;
    }
}
