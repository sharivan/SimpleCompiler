using System;

namespace compiler.types
{
    public abstract class AbstractType
    {
        protected bool resolved = false;

        public int Size => GetSize();

        protected abstract int GetSize();

        public abstract bool CoerceWith(AbstractType other, bool isExplicit);

        public override bool Equals(object other) => throw new Exception("Método não implementado");

        public override int GetHashCode() => throw new Exception("Método não implementado");

        public static bool operator ==(AbstractType t1, AbstractType t2) => ReferenceEquals(t1, t2) || t1 is not null && t2 is not null && t1.Equals(t2);

        public static bool operator !=(AbstractType t1, AbstractType t2) => !(t1 == t2);

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
    }
}
