using System.Collections.Generic;

using Asm;

namespace Comp.Types;

public class ClassType : FieldAggregationType
{
    protected readonly List<Function> methods;

    public ClassType SuperClass
    {
        get;
        internal set;
    }

    public bool IsInterface
    {
        get;
        internal set;
    }

    internal ClassType(CompilationUnity unity, string name, SourceInterval interval, int fieldAlignSize = sizeof(byte))
        : base(unity, name, interval, fieldAlignSize)
    {
        methods = new List<Function>();
    }

    public override string ToString()
    {
        string result = (IsInterface ? "interface " : "classe ") + Name + "\n{\n";

        foreach (Field field in fields)
            result += "  " + field + "\n";

        return result + "}";
    }

    public override bool CoerceWith(AbstractType other, bool isExplicit)
    {
        return Equals(other);
    }

    public override bool ContainsString()
    {
        return false;
    }

    protected override void UncheckedResolve()
    {
        size = 0;
        foreach (var field in fields)
        {
            field.Resolve();

            AbstractType type = field.Type;
            field.Offset = this.size;
            int size = type.Size;
            this.size += Compiler.GetAlignedSize(size, FieldAlignSize);
        }
    }

    protected internal override void EmitStringRelease(Context context, Compiler compiler, Assembler assembler, int offset, ReleaseType releaseType)
    {
    }
}