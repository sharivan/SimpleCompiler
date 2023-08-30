namespace Comp.Types;

public abstract class NamedType : AbstractType
{
    public CompilationUnity Unity
    {
        get;
    }

    public string Name
    {
        get;
    }

    public SourceInterval Interval
    {
        get;
    }

    protected NamedType(CompilationUnity unity, string name, SourceInterval interval)
    {
        Unity = unity;
        Name = name;
        Interval = interval;
    }
}
