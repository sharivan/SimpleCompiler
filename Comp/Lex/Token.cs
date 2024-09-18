namespace Comp.Lex;

public abstract class Token(SourceInterval interval)
{
    public SourceInterval Interval
    {
        get;
    } = interval;
}
