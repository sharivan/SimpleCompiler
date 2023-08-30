namespace Comp.Lex;

public abstract class Literal : Token
{
    protected Literal(SourceInterval interval) : base(interval)
    {
    }
}
