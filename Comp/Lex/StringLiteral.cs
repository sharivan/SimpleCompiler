namespace Comp.Lex;

public class StringLiteral : Literal
{
    public string Value
    {
        get;
    }

    internal StringLiteral(SourceInterval interval, string value) : base(interval)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value;
    }
}
