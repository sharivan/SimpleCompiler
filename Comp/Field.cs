using Comp.Types;

namespace Comp;

public class Field : Variable
{
    public StructType Container
    {
        get;
    }

    internal Field(StructType container, string name, AbstractType type, SourceInterval interval, int offset = -1) : base(name, type, interval, offset)
    {
        Container = container;
    }
}
