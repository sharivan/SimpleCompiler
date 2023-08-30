namespace Comp.Lex;

public class ByteLiteral : NumericLiteral
{
    public byte Value
    {
        get;
    }

    internal ByteLiteral(SourceInterval interval, byte value) : base(interval)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public override byte AsByte()
    {
        return Value;
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
