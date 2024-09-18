namespace Comp.Types;

public abstract class NamedType(CompilationUnity unity, string name, SourceInterval interval) : AbstractType
{
    public CompilationUnity Unity
    {
        get;
    } = unity;

    public string Name
    {
        get;
    } = name;

    public SourceInterval Interval
    {
        get;
    } = interval;
}
