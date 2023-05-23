using System;

using assembler;

namespace compiler.types;

public abstract class AbstractType
{
    internal enum ReleaseType
    {
        GLOBAL,
        LOCAL,
        PTR
    }

    protected bool resolved = false;

    public int Size => GetSize();

    protected abstract int GetSize();

    public abstract bool CoerceWith(AbstractType other, bool isExplicit);

    public override bool Equals(object other)
    {
        throw new NotImplementedException();
    }

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }

    public static bool operator ==(AbstractType t1, AbstractType t2)
    {
        return ReferenceEquals(t1, t2) || t1 is not null && t2 is not null && t1.Equals(t2);
    }

    public static bool operator !=(AbstractType t1, AbstractType t2)
    {
        return !(t1 == t2);
    }

    public abstract bool IsUnresolved();

    internal static void Resolve(ref AbstractType type)
    {
        if (!type.resolved)
        {
            type.resolved = true;
            type.UncheckedResolve();
            if (type is UnresolvedType u)
                type = u.ReferencedType;
        }
    }

    protected abstract void UncheckedResolve();

    public abstract bool ContainsString();

    internal abstract void EmitStringRelease(Context context, Compiler compiler, Assembler assembler, int offset, ReleaseType releaseType);
}
