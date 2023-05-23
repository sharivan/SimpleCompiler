using assembler;

namespace compiler.types;

public class TypeSetType : NamedType
{
    private AbstractType type;

    public AbstractType Type => type;

    internal TypeSetType(CompilationUnity unity, string name, AbstractType type, SourceInterval interval) : base(unity, name, interval)
    {
        this.type = type;
    }

    public override bool CoerceWith(AbstractType other, bool isExplicit)
    {
        return type.CoerceWith(other, isExplicit);
    }

    protected override int GetSize()
    {
        return type.Size;
    }

    public override bool IsUnresolved()
    {
        return type.IsUnresolved();
    }

    internal void Resolve()
    {
        if (!resolved)
        {
            resolved = true;
            UncheckedResolve();
        }
    }

    protected override void UncheckedResolve()
    {
        if (type is UnresolvedType u)
        {
            if (u.ReferencedType == null)
                throw new CompilerException(u.Interval, $"Tipo não declarado: '{u.Name}'.");

            type = u.ReferencedType;
        }
        else
        {
            Resolve(ref type);
        }
    }

    public override bool ContainsString()
    {
        return type.ContainsString();
    }

    internal override void EmitStringRelease(Context context, Compiler compiler, Assembler assembler, int offset, ReleaseType releaseType)
    {
        type.EmitStringRelease(context, compiler, assembler, offset, releaseType);
    }
}
