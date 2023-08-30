using System.Globalization;

namespace Comp.Lex;

public class DoubleLiteral : NumericLiteral
{
    public double Value
    {
        get;
    }

    internal DoubleLiteral(SourceInterval interval, double value) : base(interval)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value.ToString(CultureInfo.InvariantCulture);
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
        return (long) Value;
    }

    public override float AsFloat()
    {
        return (float) Value;
    }

    public override double AsDouble()
    {
        return Value;
    }
}
