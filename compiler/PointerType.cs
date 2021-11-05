using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler
{
    public class PointerType : AbstractType
    {
        public static readonly PointerType NULL = new PointerType();
        public static readonly PointerType STRING = new PointerType(PrimitiveType.CHAR);

        private AbstractType type;

        public AbstractType Type => type;

        public bool IsString => type == null || PrimitiveType.IsPrimitiveChar(type);

        public PointerType(AbstractType type = null)
        {
            this.type = type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            if (obj == null)
                return false;

            if (obj is PointerType ptr)
                return type == ptr.type;

            return false;
        }

        public override string ToString()
        {
            return (type != null ? "*" + type.ToString() : "null");
        }

        public override int Size()
        {
            return 4;
        }

        public override bool CoerceWith(AbstractType other, bool isExplicit)
        {
            if (other is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.VOID:
                    case Primitive.BOOL:
                        return false;

                    case Primitive.BYTE:
                        return isExplicit;

                    case Primitive.CHAR:
                        return false;

                    case Primitive.SHORT:
                        return isExplicit;

                    case Primitive.INT:
                        return isExplicit;

                    case Primitive.LONG:
                        return isExplicit;

                    case Primitive.FLOAT:
                        return isExplicit;

                    case Primitive.DOUBLE:
                        return isExplicit;
                }
            }

            if (other is PointerType ptr)
            {
                if (type == null)
                    return true;

                AbstractType otherType = ptr.Type;
                return isExplicit ? true : PrimitiveType.IsPrimitiveVoid(otherType) || type == otherType;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return 34944597 + EqualityComparer<AbstractType>.Default.GetHashCode(type);
        }

        public static bool operator ==(PointerType t1, PointerType t2)
        {
            if (ReferenceEquals(t1, t2))
                return true;

            if (((object) t1) == null || ((object) t2) == null)
                return false;

            return t1.Equals(t2);
        }

        public static bool operator !=(PointerType t1, PointerType t2)
        {
            return !(t1 == t2);
        }
    }
}
