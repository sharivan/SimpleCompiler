namespace compiler.lexer
{
    public abstract class Token
    {
        public SourceInterval Interval
        {
            get;
        }

        protected Token(SourceInterval interval) => Interval = interval;
    }
}
