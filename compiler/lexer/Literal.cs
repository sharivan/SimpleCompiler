namespace compiler.lexer;

public abstract class Literal : Token
{
    protected Literal(SourceInterval interval) : base(interval)
    {
    }
}
