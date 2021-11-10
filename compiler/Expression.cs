using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using compiler.types;

namespace compiler
{
    public abstract class Expression
    {
        private SourceInterval interval;

        public SourceInterval Interval => interval;

        protected Expression(SourceInterval interval)
        {
            this.interval = interval;
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
        private UnaryOperation operation;
        private Expression operand;

        public UnaryOperation Operation => operation;

        public Expression Operand => operand;

        internal UnaryExpression(SourceInterval interval, UnaryOperation operation, Expression operand) : base(interval)
        {
            this.operation = operation;
            this.operand = operand;
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
        private BinaryOperation operation;
        private Expression leftOperand;
        private Expression rightOperand;

        public BinaryOperation Operation => operation;

        public Expression LeftOperand => leftOperand;

        public Expression RightOperand => rightOperand;

        internal BinaryExpression(SourceInterval interval, BinaryOperation operation, Expression leftOperand, Expression rightOperand) : base(interval)
        {
            this.operation = operation;
            this.leftOperand = leftOperand;
            this.rightOperand = rightOperand;
        }
    }

    public class FieldAcessorExpression : Expression
    {
        private Expression operand;
        private string field;

        public Expression Operand => operand;

        public string Field => field;

        internal FieldAcessorExpression(SourceInterval interval, Expression operand, string field) : base(interval)
        {
            this.operand = operand;
            this.field = field;
        }
    }

    public class ArrayAccessorExpression : Expression
    {
        private Expression operand;
        private List<Expression> indexers;

        public Expression Operand => operand;

        public int IndexerCount => indexers.Count;

        public Expression this[int index] => indexers[index];

        internal ArrayAccessorExpression(SourceInterval interval, Expression operand) : base(interval)
        {
            this.operand = operand;

            indexers = new List<Expression>();
        }

        internal void AddIndexer(Expression indexer)
        {
            indexers.Add(indexer);
        }
    }

    public class CallExpression : Expression
    {
        private Expression operand;
        private List<Expression> parameters;

        public Expression Operand => operand;

        public int ParameterCount => parameters.Count;

        public Expression this[int index] => parameters[index];

        internal CallExpression(SourceInterval interval, Expression operand) : base(interval)
        {
            this.operand = operand;

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
        private Expression operand;

        public AbstractType Type => type;

        public Expression Operand => operand;

        internal CastExpression(SourceInterval interval, AbstractType type, Expression operand) : base(interval)
        {
            this.type = type;
            this.operand = operand;
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
        private PrimaryType primaryType;

        public PrimaryType PrimaryType => primaryType;

        public AbstractType Type => GetType();
        protected PrimaryExpression(SourceInterval interval, PrimaryType primaryType) : base(interval)
        {
            this.primaryType = primaryType;
        }

#pragma warning disable CS0108 // O membro oculta o membro herdado; nova palavra-chave ausente
        protected abstract AbstractType GetType();
#pragma warning restore CS0108 // O membro oculta o membro herdado; nova palavra-chave ausente
    }

    public class BoolLiteralExpression : PrimaryExpression
    {
        private bool value;

        public bool Value => value;

        internal BoolLiteralExpression(SourceInterval interval, bool value) : base(interval, PrimaryType.BOOL_LITERAL)
        {
            this.value = value;
        }

        protected override AbstractType GetType()
        {
            return PrimitiveType.BOOL;
        }
    }

    public class ByteLiteralExpression : PrimaryExpression
    {
        private byte value;

        public byte Value => value;

        internal ByteLiteralExpression(SourceInterval interval, byte value) : base(interval, PrimaryType.BYTE_LITERAL)
        {
            this.value = value;
        }

        protected override AbstractType GetType()
        {
            return PrimitiveType.BYTE;
        }
    }

    public class CharLiteralExpression : PrimaryExpression
    {
        private char value;

        public char Value => value;

        internal CharLiteralExpression(SourceInterval interval, char value) : base(interval, PrimaryType.CHAR_LITERAL)
        {
            this.value = value;
        }

        protected override AbstractType GetType()
        {
            return PrimitiveType.CHAR;
        }
    }

    public class ShortLiteralExpression : PrimaryExpression
    {
        private short value;

        public short Value => value;

        internal ShortLiteralExpression(SourceInterval interval, short value) : base(interval, PrimaryType.SHORT_LITERAL)
        {
            this.value = value;
        }

        protected override AbstractType GetType()
        {
            return PrimitiveType.SHORT;
        }
    }

    public class IntLiteralExpression : PrimaryExpression
    {
        private int value;

        public int Value => value;

        internal IntLiteralExpression(SourceInterval interval, int value) : base(interval, PrimaryType.INT_LITERAL)
        {
            this.value = value;
        }

        protected override AbstractType GetType()
        {
            return PrimitiveType.INT;
        }
    }

    public class LongLiteralExpression : PrimaryExpression
    {
        private long value;

        public long Value => value;

        internal LongLiteralExpression(SourceInterval interval, long value) : base(interval, PrimaryType.LONG_LITERAL)
        {
            this.value = value;
        }

        protected override AbstractType GetType()
        {
            return PrimitiveType.LONG;
        }
    }

    public class FloatLiteralExpression : PrimaryExpression
    {
        private float value;

        public float Value => value;

        internal FloatLiteralExpression(SourceInterval interval, float value) : base(interval, PrimaryType.FLOAT_LITERAL)
        {
            this.value = value;
        }

        protected override AbstractType GetType()
        {
            return PrimitiveType.FLOAT;
        }
    }

    public class DoubleLiteralExpression : PrimaryExpression
    {
        private double value;

        public double Value => value;

        internal DoubleLiteralExpression(SourceInterval interval, double value) : base(interval, PrimaryType.DOUBLE_LITERAL)
        {
            this.value = value;
        }

        protected override AbstractType GetType()
        {
            return PrimitiveType.DOUBLE;
        }
    }

    public class StringLiteralExpression : PrimaryExpression
    {
        private string value;

        public string Value => value;

        internal StringLiteralExpression(SourceInterval interval, string value) : base(interval, PrimaryType.STRING_LITERAL)
        {
            this.value = value;
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
        private string name;

        public string Name => name;

        internal IdentifierExpression(SourceInterval interval, string name) : base(interval, PrimaryType.IDENTIFIER)
        {
            this.name = name;
        }

        protected override AbstractType GetType()
        {
            throw new InvalidOperationException("Tipo desconhecido do identififcador '" + name + "'.");
        }
    }
}
