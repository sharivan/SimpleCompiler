using Comp.Types;

namespace Comp;

public class Field : Variable, IMember
{
    public FieldAggregationType DeclaringType
    {
        get;
    }

    internal Field(FieldAggregationType container, string name, AbstractType type, SourceInterval interval, int offset = -1) : base(name, type, interval, offset)
    {
        DeclaringType = container;
    }
}