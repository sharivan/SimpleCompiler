using System;

using assembler;

namespace compiler.types;

#pragma warning disable CS0659 // O tipo substitui Object. Equals (objeto o), mas não substitui o Object.GetHashCode()
public class StringType : AbstractType
#pragma warning restore CS0659 // O tipo substitui Object. Equals (objeto o), mas não substitui o Object.GetHashCode()
{
    public static readonly StringType STRING = new();

    private StringType()
    {
    }

    public override bool CoerceWith(AbstractType other, bool isExplicit)
    {
        return other is ArrayType a
            ? isExplicit && a.Type == PrimitiveType.CHAR
            : other is PointerType p && (p.Type == PrimitiveType.CHAR || p.Type == PrimitiveType.VOID && isExplicit);
    }

    public override bool Equals(object obj)
    {
        return ReferenceEquals(this, obj) || obj != null && obj is StringType;
    }

    public override bool IsUnresolved()
    {
        return false;
    }

    protected override int GetSize()
    {
        return IntPtr.Size;
    }

    protected override void UncheckedResolve()
    {
    }

    public override bool ContainsString()
    {
        return true;
    }

    internal override void EmitStringRelease(Context context, Compiler compiler, Assembler assembler, int offset, ReleaseType releaseType)
    {
        Function f = compiler.unitySystem.FindFunction("DecrementaReferenciaTexto");
        int index = compiler.GetOrAddExternalFunction(f.Name, f.ParameterSize);

        switch (releaseType)
        {
            case ReleaseType.GLOBAL:
                assembler.EmitLoadGlobalHostAddress(offset);
                break;

            case ReleaseType.LOCAL:
                assembler.EmitLoadLocalHostAddress(offset);
                break;

            case ReleaseType.PTR:
                if (offset != 0)
                {
                    assembler.EmitLoadConst(offset);
                    assembler.EmitPtrAdd();
                }

                break;
        }

        assembler.EmitLoadConst(true);
        assembler.EmitExternCall(index);
    }
}
