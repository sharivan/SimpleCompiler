using compiler.types;

namespace compiler;

public abstract class Variable
{
    private AbstractType type;

    public string Name
    {
        get;
    }

    public AbstractType Type => type;

    public SourceInterval Declaration
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

    protected Variable(string name, AbstractType type, SourceInterval declaration, int offset = -1)
    {
        Name = name;
        this.type = type;
        Declaration = declaration;
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
