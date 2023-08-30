using System;
using System.Collections.Generic;

using Comp.Types;

namespace Comp;

public abstract class Expression
{
    public SourceInterval Interval
    {
        get;
    }

    protected Expression(SourceInterval interval)
    {
        Interval = interval;
    }
}

public enum UnaryOperation
{
    NEGATION, // -a
    BITWISE_NOT, // ~a
    LOGICAL_NOT, // !a
    POINTER_INDIRECTION, // *a
    PRE_INCREMENT, // ++a
    PRE_DECREMENT, // --a
    POST_INCREMENT, // a++
    POST_DECREMENT, // a--
    POINTER_TO, // &a
}

public class UnaryExpression : Expression
{
    public UnaryOperation Operation
    {
        get;
    }

    public Expression Operand
    {
        get;
    }

    internal UnaryExpression(SourceInterval interval, UnaryOperation operation, Expression operand) : base(interval)
    {
        Operation = operation;
        Operand = operand;
    }
}

public enum BinaryOperation
{
    STORE, // a = b
    STORE_OR, // a |= b
    STORE_XOR, // a ^= b
    STORE_AND, // a &= b
    STORE_SHIFT_LEFT, // a <<= b
    STORE_SHIFT_RIGHT, // a >>= b
    STORE_UNSIGNED_SHIFT_RIGHT, // a >>>= b
    STORE_ADD, // a += b
    STORE_SUB, // a -= b
    STORE_MUL, // a *= b
    STORE_DIV, // a /= b
    STORE_MOD, // a %= b
    LOGICAL_OR, // a || b
    LOGICAL_XOR, // a ^^ b
    LOGICAL_AND, // a && b
    SHIFT_LEFT, // a << b
    SHIFT_RIGHT, // a >> b
    UNSIGNED_SHIFT_RIGHT, // a >>> b
    EQUALS, // a == b
    NOT_EQUALS, // a != b
    GREATER, // a > b
    GREATER_OR_EQUALS, // a >= b
    LESS, // a < b
    LESS_OR_EQUALS, // a <= b
    BITWISE_OR, // a | b
    BITWISE_XOR, // a ^ b
    BITWISE_AND, // a & b
    ADD, // a + b
    SUB, // a - b
    MUL, // a * b
    DIV, // a / b
    MOD // a % b
}

public class BinaryExpression : Expression
{
    public BinaryOperation Operation
    {
        get;
    }

    public Expression LeftOperand
    {
        get;
    }

    public Expression RightOperand
    {
        get;
    }

    internal BinaryExpression(SourceInterval interval, BinaryOperation operation, Expression leftOperand, Expression rightOperand) : base(interval)
    {
        Operation = operation;
        LeftOperand = leftOperand;
        RightOperand = rightOperand;
    }
}

public class FieldAccessorExpression : Expression
{
    public Expression Operand
    {
        get;
    }

    public string Field
    {
        get;
    }

    internal FieldAccessorExpression(SourceInterval interval, Expression operand, string field) : base(interval)
    {
        Operand = operand;
        Field = field;
    }
}

public class ArrayAccessorExpression : Expression
{
    private readonly List<Expression> indexers;

    public Expression Operand
    {
        get;
    }

    public int IndexerCount => indexers.Count;

    public Expression this[int index] => indexers[index];

    internal ArrayAccessorExpression(SourceInterval interval, Expression operand) : base(interval)
    {
        Operand = operand;

        indexers = new List<Expression>();
    }

    internal void AddIndexer(Expression indexer)
    {
        indexers.Add(indexer);
    }
}

public class CallExpression : Expression
{
    private readonly List<Expression> parameters;

    public Expression Operand
    {
        get;
    }

    public int ParameterCount => parameters.Count;

    public Expression this[int index] => parameters[index];

    internal CallExpression(SourceInterval interval, Expression operand) : base(interval)
    {
        Operand = operand;

        parameters = new List<Expression>();
    }

    internal void AddParameter(Expression parameter)
    {
        parameters.Add(parameter);
    }
}

public class CastExpression : Expression
{
    private AbstractType type;

    public AbstractType Type => type;

    public Expression Operand
    {
        get;
    }

    internal CastExpression(SourceInterval interval, AbstractType type, Expression operand) : base(interval)
    {
        this.type = type;
        Operand = operand;
    }

    internal void Resolve()
    {
        AbstractType.Resolve(ref type);
    }
}

public enum PrimaryType
{
    BOOL_LITERAL, // verdadeiro, falso
    BYTE_LITERAL, // 1B
    CHAR_LITERAL, // 'a'
    SHORT_LITERAL, // 1S
    INT_LITERAL, // 1
    LONG_LITERAL, // 1L
    FLOAT_LITERAL, // 1F
    DOUBLE_LITERAL, // 1.0
    STRING_LITERAL, // "abc"
    NULL_LITERAL, // null
    IDENTIFIER, // x
}

public abstract class PrimaryExpression : Expression
{
    public PrimaryType PrimaryType
    {
        get;
    }

    public AbstractType Type => GetType();
    protected PrimaryExpression(SourceInterval interval, PrimaryType primaryType) : base(interval)
    {
        PrimaryType = primaryType;
    }

#pragma warning disable CS0108 // O membro oculta o membro herdado; nova palavra-chave ausente
    protected abstract AbstractType GetType();
#pragma warning restore CS0108 // O membro oculta o membro herdado; nova palavra-chave ausente
}

public class BoolLiteralExpression : PrimaryExpression
{
    public bool Value
    {
        get;
    }

    internal BoolLiteralExpression(SourceInterval interval, bool value) : base(interval, PrimaryType.BOOL_LITERAL)
    {
        Value = value;
    }

    protected override AbstractType GetType()
    {
        return PrimitiveType.BOOL;
    }
}

public class ByteLiteralExpression : PrimaryExpression
{
    public byte Value
    {
        get;
    }

    internal ByteLiteralExpression(SourceInterval interval, byte value) : base(interval, PrimaryType.BYTE_LITERAL)
    {
        Value = value;
    }

    protected override AbstractType GetType()
    {
        return PrimitiveType.BYTE;
    }
}

public class CharLiteralExpression : PrimaryExpression
{
    public char Value
    {
        get;
    }

    internal CharLiteralExpression(SourceInterval interval, char value) : base(interval, PrimaryType.CHAR_LITERAL)
    {
        Value = value;
    }

    protected override AbstractType GetType()
    {
        return PrimitiveType.CHAR;
    }
}

public class ShortLiteralExpression : PrimaryExpression
{
    public short Value
    {
        get;
    }

    internal ShortLiteralExpression(SourceInterval interval, short value) : base(interval, PrimaryType.SHORT_LITERAL)
    {
        Value = value;
    }

    protected override AbstractType GetType()
    {
        return PrimitiveType.SHORT;
    }
}

public class IntLiteralExpression : PrimaryExpression
{
    public int Value
    {
        get;
    }

    internal IntLiteralExpression(SourceInterval interval, int value) : base(interval, PrimaryType.INT_LITERAL)
    {
        Value = value;
    }

    protected override AbstractType GetType()
    {
        return PrimitiveType.INT;
    }
}

public class LongLiteralExpression : PrimaryExpression
{
    public long Value
    {
        get;
    }

    internal LongLiteralExpression(SourceInterval interval, long value) : base(interval, PrimaryType.LONG_LITERAL)
    {
        Value = value;
    }

    protected override AbstractType GetType()
    {
        return PrimitiveType.LONG;
    }
}

public class FloatLiteralExpression : PrimaryExpression
{
    public float Value
    {
        get;
    }

    internal FloatLiteralExpression(SourceInterval interval, float value) : base(interval, PrimaryType.FLOAT_LITERAL)
    {
        Value = value;
    }

    protected override AbstractType GetType()
    {
        return PrimitiveType.FLOAT;
    }
}

public class DoubleLiteralExpression : PrimaryExpression
{
    public double Value
    {
        get;
    }

    internal DoubleLiteralExpression(SourceInterval interval, double value) : base(interval, PrimaryType.DOUBLE_LITERAL)
    {
        Value = value;
    }

    protected override AbstractType GetType()
    {
        return PrimitiveType.DOUBLE;
    }
}

public class StringLiteralExpression : PrimaryExpression
{
    public string Value
    {
        get;
    }

    internal StringLiteralExpression(SourceInterval interval, string value) : base(interval, PrimaryType.STRING_LITERAL)
    {
        Value = value;
    }

    protected override AbstractType GetType()
    {
        return PointerType.STRING;
    }
}

public class NullLiteralExpression : PrimaryExpression
{
    internal NullLiteralExpression(SourceInterval interval) : base(interval, PrimaryType.NULL_LITERAL)
    {
    }

    protected override AbstractType GetType()
    {
        return PointerType.NULL;
    }
}

public class IdentifierExpression : PrimaryExpression
{
    public string Name
    {
        get;
    }

    internal IdentifierExpression(SourceInterval interval, string name) : base(interval, PrimaryType.IDENTIFIER)
    {
        Name = name;
    }

    protected override AbstractType GetType()
    {
        throw new InvalidOperationException($"Tipo desconhecido do identififcador '{Name}'.");
    }
}
