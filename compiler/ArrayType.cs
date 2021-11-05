using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler
{
    public class ArrayType : AbstractType
    {
        private AbstractType type;
        private List<int> boundaries;

        public AbstractType Type
        {
            get
            {
                return type;
            }
        }

        public int Rank
        {
            get
            {
                return boundaries.Count;
            }
        }

        public int this[int index]
        {
            get
            {
                return boundaries[index];
            }
        }

        public ArrayType(AbstractType type)
        {
            this.type = type;

            boundaries = new List<int>();
        }

        public void AddBoundary(int boundary)
        {
            boundaries.Add(boundary);
        }

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

        public override int Size()
        {
            int result = type.Size();
            for (int i = 0; i < boundaries.Count; i++)
                result *= boundaries[i];

            return result;
        }

        public override bool CoerceWith(AbstractType other, bool isExplicit)
        {
            return Equals(other);
        }

        public override int GetHashCode()
        {
            int hashCode = 13823892;
            hashCode = hashCode * -1521134295 + EqualityComparer<AbstractType>.Default.GetHashCode(type);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<int>>.Default.GetHashCode(boundaries);
            return hashCode;
        }

        public static bool operator ==(ArrayType t1, ArrayType t2)
        {
            if (ReferenceEquals(t1, t2))
                return true;

            if (((object) t1) == null || ((object) t2) == null)
                return false;

            return t1.Equals(t2);
        }

        public static bool operator !=(ArrayType t1, ArrayType t2)
        {
            return !(t1 == t2);
        }
    }
}
