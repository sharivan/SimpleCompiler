using Comp.Types;

namespace Comp;

public class LocalVariable : Variable
{
    public SourceInterval Scope
    {
        get;
        internal set;
    }

    public Function Function
    {
        get;
    }

    public bool Param => Offset < 0;

    internal LocalVariable(Function function, string name, AbstractType type, SourceInterval declaration, int offset = -1) : base(name, type, declaration, offset)
    {
        Function = function;
        Scope = declaration;
    }

    protected internal override void Release()
    {
        base.Release();
        Function.ReleaseOffset(Offset);
    }
}