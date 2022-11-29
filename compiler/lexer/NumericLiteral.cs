namespace compiler.lexer
{
    public abstract class NumericLiteral : Literal
    {
        protected NumericLiteral(SourceInterval interval) : base(interval)
        {
        }

        public abstract byte AsByte();

        public abstract short AsShort();

        public abstract int AsInt();

        public abstract long AsLong();

        public abstract float AsFloat();

        public abstract double AsDouble();
    }
}
