using System.Collections.Generic;

namespace compiler.types
{
    public class ArrayType : AbstractType
    {
        private AbstractType type;
        private readonly List<int> boundaries;

        public AbstractType Type => type;

        public int Rank => boundaries.Count;

        public int this[int index] => boundaries[index];

        internal ArrayType(AbstractType type)
        {
            this.type = type;

            boundaries = new List<int>();
        }

        internal void AddBoundary(int boundary) => boundaries.Add(boundary);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            if (obj == null)
                return false;

            if (obj is ArrayType a)
            {
                AbstractType otherType = a.Type;
                if (type != otherType)
                    return false;

                if (boundaries.Count != a.boundaries.Count)
                    return false;

                for (int i = 0; i < boundaries.Count; i++)
                    if (boundaries[i] != a.boundaries[i])
                        return false;

                return true;
            }

            return false;
        }

        public override string ToString()
        {
            string result = type + "[";
            if (boundaries.Count > 0)
            {
                result += boundaries[0].ToString();
                for (int i = 1; i < boundaries.Count; i++)
                    result += ", " + boundaries[i];
            }

            return result + "]";
        }

        protected override int GetSize()
        {
            int result = type.Size;
            for (int i = 0; i < boundaries.Count; i++)
                result *= boundaries[i];

            return result;
        }

        public override bool CoerceWith(AbstractType other, bool isExplicit) => Equals(other);

        public override int GetHashCode()
        {
            int hashCode = 13823892;
            hashCode = hashCode * -1521134295 + EqualityComparer<AbstractType>.Default.GetHashCode(type);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<int>>.Default.GetHashCode(boundaries);
            return hashCode;
        }

        public static bool operator ==(ArrayType t1, ArrayType t2) => ReferenceEquals(t1, t2) || t1 is not null && t2 is not null && t1.Equals(t2);

        public static bool operator !=(ArrayType t1, ArrayType t2) => !(t1 == t2);

        public override bool IsUnresolved() => type.IsUnresolved();

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
                Resolve(ref type);
        }
    }
}
