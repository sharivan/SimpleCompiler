using System.Globalization;

namespace compiler.lexer;

public class FloatLiteral : NumericLiteral
{
    public float Value
    {
        get;
    }

    internal FloatLiteral(SourceInterval interval, float value) : base(interval)
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
        return Value;
    }

    public override double AsDouble()
    {
        return Value;
    }
}
