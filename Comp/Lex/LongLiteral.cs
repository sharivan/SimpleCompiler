namespace Comp.Lex;

public class LongLiteral : NumericLiteral
{
    public long Value
    {
        get;
    }

    internal LongLiteral(SourceInterval interval, long value) : base(interval)
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
        return (int) Value;
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
