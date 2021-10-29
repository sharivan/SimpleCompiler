using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler
{
    public enum Primitive
    {
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
        public static readonly PrimitiveType BOOL = new PrimitiveType(Primitive.BOOL);
        public static readonly PrimitiveType BYTE = new PrimitiveType(Primitive.BYTE);
        public static readonly PrimitiveType CHAR = new PrimitiveType(Primitive.CHAR);
        public static readonly PrimitiveType SHORT = new PrimitiveType(Primitive.SHORT);
        public static readonly PrimitiveType INT = new PrimitiveType(Primitive.INT);
        public static readonly PrimitiveType LONG = new PrimitiveType(Primitive.LONG);
        public static readonly PrimitiveType FLOAT = new PrimitiveType(Primitive.FLOAT);
        public static readonly PrimitiveType DOUBLE = new PrimitiveType(Primitive.DOUBLE);

        public static bool IsPrimitiveBool(PrimitiveType type)
        {
            return type.primitive == Primitive.BOOL;
        }

        public static bool IsPrimitiveNumber(PrimitiveType type)
        {
            return IsPrimitiveInteger(type) || IsPrimitiveFloat(type);
        }

        public static bool IsPrimitiveInteger(PrimitiveType type)
        {
            return type.primitive == Primitive.BYTE || type.primitive == Primitive.SHORT || type.primitive == Primitive.INT || type.primitive == Primitive.LONG;
        }

        public static bool IsPrimitiveFloat(PrimitiveType type)
        {
            return type.primitive == Primitive.FLOAT || type.primitive == Primitive.DOUBLE;
        }

        public static bool IsUpTo32BitsInt(PrimitiveType type)
        {
            return type.primitive == Primitive.BYTE || type.primitive == Primitive.SHORT || type.primitive == Primitive.INT;
        }

        public static bool Is64BitsInt(PrimitiveType type)
        {
            return type.primitive == Primitive.LONG;
        }

        public static bool Is32BitsFloat(PrimitiveType type)
        {
            return type.primitive == Primitive.FLOAT;
        }

        public static bool Is64BitsFloat(PrimitiveType type)
        {
            return type.primitive == Primitive.DOUBLE;
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

        public override string ToString()
        {
            switch (primitive)
            {
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
    }
}
