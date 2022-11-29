namespace compiler.lexer
{
    public class ByteLiteral : NumericLiteral
    {
        public byte Value
        {
            get;
        }

        internal ByteLiteral(SourceInterval interval, byte value) : base(interval) => Value = value;

        public override string ToString() => Value.ToString();

        public override byte AsByte() => Value;

        public override short AsShort() => Value;

        public override int AsInt() => Value;

        public override long AsLong() => Value;

        public override float AsFloat() => Value;

        public override double AsDouble() => Value;
    }
}
