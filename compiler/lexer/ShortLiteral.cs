namespace compiler.lexer;

public class ShortLiteral : NumericLiteral
{
    public short Value
    {
        get;
    }

    internal ShortLiteral(SourceInterval interval, short value) : base(interval)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public override byte AsByte()
    {
        return (byte) Value;
    }

    public override short AsShort()
    {
        return Value;
    }

    public override int AsInt()
    {
        return Value;
    }

    public override long AsLong()
    {
        return (long) Value;
    }

    public override float AsFloat()
    {
        return Value;
    }

    public override double AsDouble()
    {
        return Value;
    }
}
