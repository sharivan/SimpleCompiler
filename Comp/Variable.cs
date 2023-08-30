using Comp.Types;

namespace Comp;

public abstract class Variable
{
    private AbstractType type;

    public string Name
    {
        get;
    }

    public AbstractType Type => type;

    public SourceInterval Interval
    {
        get;
    }

    public int Offset
    {
        get; internal set;
    }

    public bool Temporary
    {
        get; internal set;
    }

    public bool Acquired
    {
        get; internal set;
    }

    protected Variable(string name, AbstractType type, SourceInterval interval, int offset = -1)
    {
        Name = name;
        this.type = type;
        Interval = interval;
        Offset = offset;
    }

    public override string ToString()
    {
        return $"{Name}:{type} ({Offset})";
    }

    internal void Resolve()
    {
        AbstractType.Resolve(ref type);
    }

    internal void Release()
    {
        Acquired = false;
    }
}
