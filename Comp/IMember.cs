using Comp.Types;

namespace Comp;

public interface IMember
{
    public FieldAggregationType DeclaringType
    {
        get;
    }
}