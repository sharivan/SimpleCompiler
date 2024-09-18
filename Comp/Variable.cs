using Comp.Types;

namespace Comp;

public abstract class Variable(string name, AbstractType type, SourceInterval interval, int offset = -1)
{
    private AbstractType type = type;

    public string Name
    {
        get;
    } = name;

    public AbstractType Type => type;

    public SourceInterval Interval
    {
        get;
    } = interval;

    public int Offset
    {
        get; internal set;
    } = offset;

    public bool Temporary
    {
        get; internal set;
    }

    public bool Acquired
    {
        get; internal set;
    }

    public override string ToString()
    {
        return $"{Name}:{type} ({Offset})";
    }

    internal void Resolve()
    {
        AbstractType.Resolve(ref type);
    }

    protected internal virtual void Release()
    {
        Acquired = false;
    }
}