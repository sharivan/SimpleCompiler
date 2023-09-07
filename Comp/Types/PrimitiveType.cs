using System;

using Asm;

namespace Comp.Types;

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
    public static readonly PrimitiveType VOID = new(Primitive.VOID);
    public static readonly PrimitiveType BOOL = new(Primitive.BOOL);
    public static readonly PrimitiveType BYTE = new(Primitive.BYTE);
    public static readonly PrimitiveType CHAR = new(Primitive.CHAR);
    public static readonly PrimitiveType SHORT = new(Primitive.SHORT);
    public static readonly PrimitiveType INT = new(Primitive.INT);
    public static readonly PrimitiveType LONG = new(Primitive.LONG);
    public static readonly PrimitiveType FLOAT = new(Primitive.FLOAT);
    public static readonly PrimitiveType DOUBLE = new(Primitive.DOUBLE);

    public static PrimitiveType FromPrimitive(Primitive primitive)
    {
        return primitive switch
        {
            Primitive.VOID => VOID,
            Primitive.BOOL => BOOL,
            Primitive.BYTE => BYTE,
            Primitive.CHAR => CHAR,
            Primitive.SHORT => SHORT,
            Primitive.INT => INT,
            Primitive.LONG => LONG,
            Primitive.FLOAT => FLOAT,
            Primitive.DOUBLE => DOUBLE,
            _ => null,
        };
    }

    public static bool IsPrimitiveVoid(PrimitiveType type)
    {
        return type.Primitive == Primitive.VOID;
    }

    public static bool IsPrimitiveVoid(AbstractType type)
    {
        return type is PrimitiveType p && IsPrimitiveVoid(p);
    }

    public static bool IsPrimitiveBool(PrimitiveType type)
    {
        return type.Primitive == Primitive.BOOL;
    }

    public static bool IsPrimitiveBool(AbstractType type)
    {
        return type is PrimitiveType p && IsPrimitiveBool(p);
    }

    public static bool IsPrimitiveNumber(PrimitiveType type)
    {
        return IsPrimitiveInteger(type) || IsPrimitiveFloat(type);
    }

    public static bool IsPrimitiveNumber(AbstractType type)
    {
        return type is PrimitiveType p && IsPrimitiveNumber(p);
    }

    public static bool IsPrimitiveInteger(PrimitiveType type)
    {
        return type.Primitive is Primitive.BYTE or Primitive.SHORT or Primitive.INT or Primitive.LONG;
    }

    public static bool IsPrimitiveInteger(AbstractType type)
    {
        return type is PrimitiveType p && IsPrimitiveInteger(p);
    }

    public static bool IsPrimitiveChar(PrimitiveType type)
    {
        return type.Primitive == Primitive.CHAR;
    }

    public static bool IsPrimitiveChar(AbstractType type)
    {
        return type is PrimitiveType p && IsPrimitiveChar(p);
    }

    public static bool IsPrimitiveFloat(PrimitiveType type)
    {
        return type.Primitive is Primitive.FLOAT or Primitive.DOUBLE;
    }

    public static bool IsPrimitiveFloat(AbstractType type)
    {
        return type is PrimitiveType p && IsPrimitiveFloat(p);
    }

    public static bool IsUpTo32BitsInt(PrimitiveType type)
    {
        return type.Primitive is Primitive.BYTE or Primitive.SHORT or Primitive.INT;
    }

    public static bool IsUpTo32BitsInt(AbstractType type)
    {
        return type is PrimitiveType p && IsUpTo32BitsInt(p);
    }

    public static bool Is64BitsInt(PrimitiveType type)
    {
        return type.Primitive == Primitive.LONG;
    }

    public static bool Is64BitsInt(AbstractType type)
    {
        return type is PrimitiveType p && Is64BitsInt(p);
    }

    public static bool Is32BitsFloat(PrimitiveType type)
    {
        return type.Primitive == Primitive.FLOAT;
    }

    public static bool Is32BitsFloat(AbstractType type)
    {
        return type is PrimitiveType p && Is32BitsFloat(p);
    }

    public static bool Is64BitsFloat(PrimitiveType type)
    {
        return type.Primitive == Primitive.DOUBLE;
    }

    public static bool Is64BitsFloat(AbstractType type)
    {
        return type is PrimitiveType p && Is64BitsFloat(p);
    }

    public Primitive Primitive
    {
        get;
    }

    private PrimitiveType(Primitive primitive)
    {
        Primitive = primitive;
    }

    public override bool Equals(object obj)
    {
        return ReferenceEquals(this, obj) || obj != null && obj is PrimitiveType p && Primitive == p.Primitive;
    }

    public override string ToString()
    {
        return Primitive switch
        {
            Primitive.VOID => "void",
            Primitive.BOOL => "bool",
            Primitive.BYTE => "byte",
            Primitive.CHAR => "char",
            Primitive.SHORT => "short",
            Primitive.INT => "int",
            Primitive.LONG => "long",
            Primitive.FLOAT => "float",
            Primitive.DOUBLE => "real",
            _ => throw new Exception("Unknow primitive type."),
        };
    }

    protected override int GetSize()
    {
        return Primitive switch
        {
            Primitive.VOID => 0,
            Primitive.BOOL => sizeof(bool),
            Primitive.BYTE => sizeof(byte),
            Primitive.CHAR => sizeof(char),
            Primitive.SHORT => sizeof(short),
            Primitive.INT => sizeof(int),
            Primitive.LONG => sizeof(long),
            Primitive.FLOAT => sizeof(float),
            Primitive.DOUBLE => sizeof(double),
            _ => throw new Exception("Unknow primitive type."),
        };
    }

    public override bool CoerceWith(AbstractType other, bool isExplicit)
    {
        if (other is PrimitiveType o)
        {
            switch (Primitive)
            {
                case Primitive.VOID:
                    return false;

                case Primitive.BOOL:
                    return isExplicit || o.Primitive == Primitive.BOOL;

                case Primitive.BYTE:
                    return isExplicit ? o.Primitive != Primitive.BOOL : o.Primitive is >= Primitive.BYTE and not Primitive.CHAR;

                case Primitive.CHAR:
                    return isExplicit ? o.Primitive != Primitive.BOOL : o.Primitive == Primitive.CHAR;

                case Primitive.SHORT:
                    return isExplicit ? o.Primitive != Primitive.BOOL : o.Primitive >= Primitive.SHORT;

                case Primitive.INT:
                    return isExplicit ? o.Primitive != Primitive.BOOL : o.Primitive >= Primitive.INT;

                case Primitive.LONG:
                    return isExplicit ? o.Primitive != Primitive.BOOL : o.Primitive >= Primitive.LONG;

                case Primitive.FLOAT:
                    return isExplicit ? o.Primitive != Primitive.BOOL : o.Primitive >= Primitive.FLOAT;

                case Primitive.DOUBLE:
                    return isExplicit ? o.Primitive != Primitive.BOOL : o.Primitive == Primitive.DOUBLE;
            }
        }

        if (other is PointerType)
        {
            switch (Primitive)
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
        return 1968834918 + Primitive.GetHashCode();
    }

    public static bool operator ==(PrimitiveType t1, PrimitiveType t2)
    {
        return ReferenceEquals(t1, t2) || t1 is not null && t2 is not null && t1.Equals(t2);
    }

    public static bool operator !=(PrimitiveType t1, PrimitiveType t2)
    {
        return !(t1 == t2);
    }

    public override bool IsUnresolved()
    {
        return false;
    }

    protected override void UncheckedResolve()
    {
    }

    public override bool ContainsString()
    {
        return false;
    }

    protected internal override void EmitStringRelease(Context context, Compiler compiler, Assembler assembler, int offset, ReleaseType releaseType)
    {
    }
}