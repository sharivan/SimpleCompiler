namespace Comp.Lex;

public abstract class NumericLiteral(SourceInterval interval) : Literal(interval)
{
    public abstract byte AsByte();

    public abstract short AsShort();

    public abstract int AsInt();

    public abstract long AsLong();

    public abstract float AsFloat();

    public abstract double AsDouble();
}
