namespace Comp.Lex;

public class IntLiteral : NumericLiteral
{
    public int Value
    {
        get;
    }

    internal IntLiteral(SourceInterval interval, int value) : base(interval)
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
        return (short) Value;
    }

    public override int AsInt()
    {
        return Value;
    }

    public override long AsLong()
    {
        return Value;
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
