namespace compiler.lexer;

public class CharLiteral : Literal
{
    public char Value
    {
        get;
    }

    internal CharLiteral(SourceInterval interval, char value) : base(interval)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
