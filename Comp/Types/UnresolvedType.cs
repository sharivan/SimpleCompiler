using Asm;

namespace Comp.Types;

public class UnresolvedType : NamedType
{
    private AbstractType referencedType;

    public AbstractType ReferencedType
    {
        get => referencedType;
        internal set => referencedType = value;
    }

    internal UnresolvedType(CompilationUnity unity, string name, SourceInterval interval) : base(unity, name, interval)
    {
    }

    public override bool CoerceWith(AbstractType other, bool isExplicit)
    {
        throw new CompilerException(Interval, $"Tipo não resolvido: '{Name}'.");
    }

    protected override int GetSize()
    {
        throw new CompilerException(Interval, $"Tipo não resolvido: '{Name}'.");
    }

    public override bool IsUnresolved()
    {
        return referencedType == null;
    }

    protected override void UncheckedResolve()
    {
        if (referencedType == null)
        {
            referencedType = Unity.FindStruct(Name);
            if (referencedType == null)
                throw new CompilerException(Interval, $"Tipo não declarado: '{Name}'.");

            Resolve(ref referencedType);
        }
    }

    public override bool ContainsString()
    {
        throw new CompilerException(Interval, $"Tipo não resolvido: '{Name}'.");
    }

    internal override void EmitStringRelease(Context context, Compiler compiler, Assembler assembler, int offset, ReleaseType releaseType)
    {
        throw new CompilerException(Interval, $"Tipo não resolvido: '{Name}'.");
    }
}
