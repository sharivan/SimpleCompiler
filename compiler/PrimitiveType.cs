using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler
{
    public enum Primitive
    {
        VOID,
        BOOL,
        BYTE,
        CHAR,
        SHORT,
        INT,
        LONG,
        FLOAT,
        DOUBLE
    }

    public class PrimitiveType : AbstractType
    {
        public static readonly PrimitiveType VOID = new PrimitiveType(Primitive.VOID);
        public static readonly PrimitiveType BOOL = new PrimitiveType(Primitive.BOOL);
        public static readonly PrimitiveType BYTE = new PrimitiveType(Primitive.BYTE);
        public static readonly PrimitiveType CHAR = new PrimitiveType(Primitive.CHAR);
        public static readonly PrimitiveType SHORT = new PrimitiveType(Primitive.SHORT);
        public static readonly PrimitiveType INT = new PrimitiveType(Primitive.INT);
        public static readonly PrimitiveType LONG = new PrimitiveType(Primitive.LONG);
        public static readonly PrimitiveType FLOAT = new PrimitiveType(Primitive.FLOAT);
        public static readonly PrimitiveType DOUBLE = new PrimitiveType(Primitive.DOUBLE);

        public static bool IsPrimitiveVoid(PrimitiveType type)
        {
            return type.primitive == Primitive.VOID;
        }

        public static bool IsPrimitiveVoid(AbstractType type)
        {
            if (type is PrimitiveType p)
                return IsPrimitiveVoid(p);

            return false;
        }

        public static bool IsPrimitiveBool(PrimitiveType type)
        {
            return type.primitive == Primitive.BOOL;
        }

        public static bool IsPrimitiveBool(AbstractType type)
        {
            if (type is PrimitiveType p)
                return IsPrimitiveBool(p);

            return false;
        }

        public static bool IsPrimitiveNumber(PrimitiveType type)
        {
            return IsPrimitiveInteger(type) || IsPrimitiveFloat(type);
        }

        public static bool IsPrimitiveNumber(AbstractType type)
        {
            if (type is PrimitiveType p)
                return IsPrimitiveNumber(p);

            return false;
        }

        public static bool IsPrimitiveInteger(PrimitiveType type)
        {
            return type.primitive == Primitive.BYTE || type.primitive == Primitive.SHORT || type.primitive == Primitive.INT || type.primitive == Primitive.LONG;
        }

        public static bool IsPrimitiveInteger(AbstractType type)
        {
            if (type is PrimitiveType p)
                return IsPrimitiveInteger(p);

            return false;
        }

        public static bool IsPrimitiveChar(PrimitiveType type)
        {
            return type.primitive == Primitive.CHAR;
        }

        public static bool IsPrimitiveChar(AbstractType type)
        {
            if (type is PrimitiveType p)
                return IsPrimitiveChar(p);

            return false;
        }

        public static bool IsPrimitiveFloat(PrimitiveType type)
        {
            return type.primitive == Primitive.FLOAT || type.primitive == Primitive.DOUBLE;
        }

        public static bool IsPrimitiveFloat(AbstractType type)
        {
            if (type is PrimitiveType p)
                return IsPrimitiveFloat(p);

            return false;
        }

        public static bool IsUpTo32BitsInt(PrimitiveType type)
        {
            return type.primitive == Primitive.BYTE || type.primitive == Primitive.SHORT || type.primitive == Primitive.INT;
        }

        public static bool IsUpTo32BitsInt(AbstractType type)
        {
            if (type is PrimitiveType p)
                return IsUpTo32BitsInt(p);

            return false;
        }

        public static bool Is64BitsInt(PrimitiveType type)
        {
            return type.primitive == Primitive.LONG;
        }

        public static bool Is64BitsInt(AbstractType type)
        {
            if (type is PrimitiveType p)
                return Is64BitsInt(p);

            return false;
        }

        public static bool Is32BitsFloat(PrimitiveType type)
        {
            return type.primitive == Primitive.FLOAT;
        }

        public static bool Is32BitsFloat(AbstractType type)
        {
            if (type is PrimitiveType p)
                return Is32BitsFloat(p);

            return false;
        }

        public static bool Is64BitsFloat(PrimitiveType type)
        {
            return type.primitive == Primitive.DOUBLE;
        }

        public static bool Is64BitsFloat(AbstractType type)
        {
            if (type is PrimitiveType p)
                return Is64BitsFloat(p);

            return false;
        }

        private Primitive primitive;

        public Primitive Primitive
        {
            get
            {
                return primitive;
            }
        }

        private PrimitiveType(Primitive primitive)
        {
            this.primitive = primitive;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            if (obj == null)
                return false;

            if (obj is PrimitiveType p)
                return primitive == p.primitive;

            return false;
        }

        public override string ToString()
        {
            switch (primitive)
            {
                case Primitive.VOID:
                    return "void";

                case Primitive.BOOL:
                    return "bool";

                case Primitive.BYTE:
                    return "byte";

                case Primitive.CHAR:
                    return "char";

                case Primitive.SHORT:
                    return "short";

                case Primitive.INT:
                    return "int";

                case Primitive.LONG:
                    return "long";

                case Primitive.FLOAT:
                    return "float";

                case Primitive.DOUBLE:
                    return "real";
            }

            return "?";
        }

        public override int Size()
        {
            switch (primitive)
            {
                case Primitive.VOID:
                    return 0;

                case Primitive.BOOL:
                    return 1;

                case Primitive.BYTE:
                    return 1;

                case Primitive.CHAR:
                    return 2;

                case Primitive.SHORT:
                    return 2;

                case Primitive.INT:
                    return 4;

                case Primitive.LONG:
                    return 8;

                case Primitive.FLOAT:
                    return 4;

                case Primitive.DOUBLE:
                    return 8;
            }

            return -1;
        }

        public override bool CoerceWith(AbstractType other, bool isExplicit)
        {
            if (other is PrimitiveType o)
            {
                switch (primitive)
                {
                    case Primitive.VOID:
                        return false;

                    case Primitive.BOOL:
                        return isExplicit ? true : o.primitive == Primitive.BOOL;

                    case Primitive.BYTE:
                        return isExplicit ? o.primitive != Primitive.BOOL : o.primitive >= Primitive.BYTE && o.primitive != Primitive.CHAR;

                    case Primitive.CHAR:
                        return isExplicit ? o.primitive != Primitive.BOOL : o.primitive == Primitive.CHAR;

                    case Primitive.SHORT:
                        return isExplicit ? o.primitive != Primitive.BOOL : o.primitive >= Primitive.SHORT;

                    case Primitive.INT:
                        return isExplicit ? o.primitive != Primitive.BOOL : o.primitive >= Primitive.INT;

                    case Primitive.LONG:
                        return isExplicit ? o.primitive != Primitive.BOOL : o.primitive >= Primitive.LONG;

                    case Primitive.FLOAT:
                        return isExplicit ? o.primitive != Primitive.BOOL : o.primitive >= Primitive.FLOAT;

                    case Primitive.DOUBLE:
                        return isExplicit ? o.primitive != Primitive.BOOL : o.primitive == Primitive.DOUBLE;
                }
            }

            if (other is PointerType)
            {
                switch (primitive)
                {
                    case Primitive.VOID:
                    case Primitive.BOOL:
                    case Primitive.BYTE:
                    case Primitive.CHAR:
                    case Primitive.SHORT:
                        return false;

                    case Primitive.INT:
                        return isExplicit;

                    case Primitive.LONG:
                        return isExplicit;

                    case Primitive.FLOAT:
                    case Primitive.DOUBLE:
                        return false;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return 1968834918 + primitive.GetHashCode();
        }

        public static bool operator ==(PrimitiveType t1, PrimitiveType t2)
        {
            if (ReferenceEquals(t1, t2))
                return true;

            if (((object) t1) == null || ((object) t2) == null)
                return false;

            return t1.Equals(t2);
        }

        public static bool operator !=(PrimitiveType t1, PrimitiveType t2)
        {
            return !(t1 == t2);
        }
    }
}
