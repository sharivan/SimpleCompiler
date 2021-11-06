using System;
using System.Collections.Generic;
using System.Text;

namespace compiler
{
    public class Compiler
    {
        public static int GetSizeInDWords(int sizeInBytes)
        {
            int r = sizeInBytes % 4;
            int d = sizeInBytes / 4;
            if (r != 0)
                return d + 1;

            return d;
        }

        private Lexer lexer;
        private List<GlobalVariable> globals;
        private Dictionary<string, GlobalVariable> globalTable;
        private List<StructType> structs;
        private Dictionary<string, StructType> structTable;
        private List<Function> functions;
        private Dictionary<string, Function> functionTable;
        private List<Label> labels;
        private Dictionary<string, int> stringTable;

        private int globalVariableOffset;
        private Function entryPoint;

        public Compiler()
        {
            lexer = new Lexer();
            globals = new List<GlobalVariable>();
            globalTable = new Dictionary<string, GlobalVariable>();
            structs = new List<StructType>();
            structTable = new Dictionary<string, StructType>();
            functions = new List<Function>();
            functionTable = new Dictionary<string, Function>();
            labels = new List<Label>();
            stringTable = new Dictionary<string, int>();

            globalVariableOffset = 1;
            entryPoint = null;
        }
        
        public Label CreateLabel()
        {
            Label result = new Label();
            labels.Add(result);
            return result;
        }

        public int GetStringOffset(string value)
        {
            if (stringTable.ContainsKey(value))
                return stringTable[value];

            int size = (value.Length + 1) * 2;
            int offset = globalVariableOffset;
            stringTable.Add(value, offset);
            globalVariableOffset += size;
            return offset;
        }

        public GlobalVariable FindGlobalVariable(string name)
        {
            if (globalTable.TryGetValue(name, out GlobalVariable result))
                return result;

            return null;
        }

        public GlobalVariable DeclareGlobalVariable(string name, AbstractType type)
        {
            GlobalVariable result = FindGlobalVariable(name);
            if (result != null)
                return null;

            result = new GlobalVariable(name, type, globalVariableOffset);
            globalVariableOffset += type.Size();
            globals.Add(result);
            globalTable.Add(name, result);
            return result;
        }

        public GlobalVariable DeclareGlobalVariable(string name, AbstractType type, object initialValue)
        {
            GlobalVariable result = FindGlobalVariable(name);
            if (result != null)
                return null;

            result = new GlobalVariable(name, type, globalVariableOffset, initialValue);
            globalVariableOffset += type.Size();
            globals.Add(result);
            globalTable.Add(name, result);
            return result;
        }

        public StructType FindStruct(string name)
        {
            if (structTable.TryGetValue(name, out StructType result))
                return result;

            return null;
        }

        public StructType DeclareStruct(string name)
        {
            StructType result = FindStruct(name);
            if (result != null)
                return null;

            result = new StructType(name);
            structs.Add(result);
            structTable.Add(name, result);
            return result;
        }

        public Function FindFunction(string name)
        {
            if (functionTable.TryGetValue(name, out Function result))
                return result;

            return null;
        }

        public Function DeclareFunction(string name)
        {
            Function result = FindFunction(name);
            if (result != null)
                return null;

            result = new Function(this, name);
            functions.Add(result);
            functionTable.Add(name, result);
            return result;
        }

        private void CompileInt32Conversion(Assembler assembler, PrimitiveType toType)
        {
            switch (toType.Primitive)
            {
                case Primitive.LONG:
                    assembler.EmitInt32ToInt64();
                    break;

                case Primitive.FLOAT:
                    assembler.EmitInt32ToFloat32();
                    break;

                case Primitive.DOUBLE:
                    assembler.EmitInt32ToFloat64();
                    break;
            }
        }

        private void CompileCast(Assembler assembler, AbstractType fromType, AbstractType toType, bool isExplicit, SourceInterval interval)
        {
            if (fromType is PrimitiveType p)
            {
                if (toType is PrimitiveType tp)
                {
                    switch (p.Primitive)
                    {
                        case Primitive.VOID:
                            throw new CompilerException(interval, "Conversão inválida de 'void' para '" + tp + "'.");

                        case Primitive.BOOL:
                            if (isExplicit ? false : tp.Primitive != Primitive.BOOL)
                                throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            // TODO Implementar
                            break;

                        case Primitive.BYTE:
                            if (isExplicit ? tp.Primitive == Primitive.BOOL : tp.Primitive < Primitive.BYTE || tp.Primitive == Primitive.CHAR)
                                throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            CompileInt32Conversion(assembler, tp);
                            break;

                        case Primitive.CHAR:
                            if (isExplicit ? tp.Primitive != Primitive.BOOL : tp.Primitive != Primitive.CHAR)
                                throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            CompileInt32Conversion(assembler, tp);
                            break;

                        case Primitive.SHORT:
                            if (isExplicit ? tp.Primitive == Primitive.BOOL : tp.Primitive < Primitive.SHORT)
                                throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            CompileInt32Conversion(assembler, tp);
                            break;

                        case Primitive.INT:
                            if (isExplicit ? tp.Primitive == Primitive.BOOL : tp.Primitive < Primitive.INT)
                                throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            CompileInt32Conversion(assembler, tp);
                            break;

                        case Primitive.LONG:
                            if (isExplicit ? tp.Primitive == Primitive.BOOL : tp.Primitive < Primitive.LONG)
                                throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            switch (tp.Primitive)
                            {
                                case Primitive.BYTE:
                                case Primitive.CHAR:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitInt64ToInt32();
                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitInt64ToFloat64();
                                    assembler.EmitFloat64ToFloat32();
                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitInt64ToFloat64();
                                    break;
                            }

                            break;

                        case Primitive.FLOAT:
                            if (isExplicit ? tp.Primitive == Primitive.BOOL : tp.Primitive < Primitive.FLOAT)
                                throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            switch (tp.Primitive)
                            {
                                case Primitive.BYTE:
                                case Primitive.CHAR:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitFloat32ToInt32();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitFloat32ToInt64();
                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitFloat32ToFloat64();
                                    break;
                            }

                            break;

                        case Primitive.DOUBLE:
                            if (isExplicit ? tp.Primitive == Primitive.BOOL : tp.Primitive < Primitive.DOUBLE)
                                throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            switch (tp.Primitive)
                            {
                                case Primitive.BYTE:
                                case Primitive.CHAR:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitFloat64ToInt64();
                                    assembler.EmitInt64ToInt32();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitFloat64ToInt64();
                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitFloat64ToFloat32();
                                    break;
                            }

                            break;
                    }

                    return;
                }

                if (toType is PointerType)
                {
                    switch (p.Primitive)
                    {
                        case Primitive.VOID:
                        case Primitive.BOOL:
                        case Primitive.BYTE:
                        case Primitive.CHAR:
                        case Primitive.SHORT:
                            throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                        case Primitive.INT:
                            if (!isExplicit)
                                throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            break;

                        case Primitive.LONG:
                            if (!isExplicit)
                                throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            assembler.EmitInt64ToInt32();
                            break;

                        case Primitive.FLOAT:
                        case Primitive.DOUBLE:
                            throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");
                    }

                    return;
                }

                throw new CompilerException(interval, "Tipo desconhecido: '" + toType + "'.");
            }

            if (fromType is StructType s)
            {
                if (!s.Equals(toType))
                    throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                return;
            }

            if (fromType is ArrayType a)
            {
                if (!a.Equals(toType))
                    throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                return;
            }

            if (fromType is PointerType fptr)
            {
                if (toType is PrimitiveType tp)
                {
                    switch (tp.Primitive)
                    {
                        case Primitive.VOID:
                        case Primitive.BOOL:
                            throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                        case Primitive.BYTE:
                            if (!isExplicit)
                                throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            return;

                        case Primitive.CHAR:
                            throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                        case Primitive.SHORT:
                            if (!isExplicit)
                                throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            return;

                        case Primitive.INT:
                            if (!isExplicit)
                                throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            return;

                        case Primitive.LONG:
                            if (!isExplicit)
                                throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            assembler.EmitInt32ToInt64();
                            return;

                        case Primitive.FLOAT:
                            if (!isExplicit)
                                throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            assembler.EmitInt32ToFloat32();
                            return;

                        case Primitive.DOUBLE:
                            if (!isExplicit)
                                throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            assembler.EmitInt32ToFloat64();
                            return;
                    }

                    throw new CompilerException(interval, "Tipo desconhecido: '" + toType + "'.");
                }

                if (toType is PointerType tptr)
                {
                    if (fptr.Type == null)
                        return;

                    AbstractType otherType = tptr.Type;
                    if (isExplicit ? false : !PrimitiveType.IsPrimitiveVoid(otherType) && fptr.Type != otherType)
                        throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                    return;
                }

                throw new CompilerException(interval, "Tipo desconhecido: '" + toType + "'.");
            }

            throw new CompilerException(interval, "Tipo desconhecido: '" + fromType + "'.");
        }

        private Expression ParsePrimaryExpression()
        {
            Token token = lexer.NextToken();
            if (token == null)
                throw new CompilerException(lexer.CurrentInterval(lexer.CurrentPos), "Fim do arquivo encontrado mas token esperado.");

            if (token is Keyword kw)
            {
                switch (kw.Value)
                {
                    case "verdade":
                        return new BoolLiteralExpression(kw.Interval, true);

                    case "falso":
                        return new BoolLiteralExpression(kw.Interval, false);

                    case "nulo":
                        return new NullLiteralExpression(kw.Interval);
                }
            }

            if (token is ByteLiteral b)
                return new ByteLiteralExpression(token.Interval, b.Value);

            if (token is CharLiteral c)
                return new CharLiteralExpression(token.Interval, c.Value);

            if (token is ShortLiteral s)
                return new ShortLiteralExpression(token.Interval, s.Value);

            if (token is IntLiteral i)
                return new IntLiteralExpression(token.Interval, i.Value);

            if (token is LongLiteral l)
                return new LongLiteralExpression(token.Interval, l.Value);

            if (token is FloatLiteral f)
                return new FloatLiteralExpression(token.Interval, f.Value);

            if (token is DoubleLiteral d)
                return new DoubleLiteralExpression(token.Interval, d.Value);

            if (token is StringLiteral str)
                return new StringLiteralExpression(token.Interval, str.Value);

            if (token is Identifier id)
            {
                if (lexer.NextSymbol("(", false) != null)
                {
                    CallExpression result = new CallExpression(id.Interval, new IdentifierExpression(id.Interval, id.Name));

                    if (lexer.NextSymbol(")", false) != null)
                        return result;

                    do
                    {
                        Expression parameter = ParseExpression();
                        result.AddParameter(parameter);
                    }
                    while (lexer.NextSymbol(",", false) != null);

                    lexer.NextSymbol(")");

                    return result;
                }

                return new IdentifierExpression(id.Interval, id.Name);
            }

            if (token is Symbol symbol)
            {
                if (symbol.Value != "(")
                    throw new CompilerException(symbol.Interval, "'(' esperado mas '" + symbol.Value + "' encontrado.");

                Expression result = ParseExpression();

                lexer.NextSymbol(")");

                return result;
            }

            throw new CompilerException(token.Interval, "Token não esperado: " + token);
        }

        private Expression ParsePostFixExpression()
        {
            Expression operand = ParsePrimaryExpression();

            Symbol symbol = lexer.NextSymbol(false);
            if (symbol == null)
                return operand;

            switch (symbol.Value)
            {
                case "++":
                    return new UnaryExpression(SourceInterval.Merge(operand.Interval, symbol.Interval), UnaryOperation.POST_INCREMENT, operand);

                case "--":
                    return new UnaryExpression(SourceInterval.Merge(operand.Interval, symbol.Interval), UnaryOperation.POST_DECREMENT, operand);

                case ".":
                {
                    Identifier id = lexer.NextIdentifier();
                    return new FieldAcessorExpression(SourceInterval.Merge(operand.Interval, id.Interval), operand, id.Name);
                }

                case "[":
                {
                    if (lexer.NextSymbol("]", false) != null)
                        throw new CompilerException(operand.Interval, "Índice de array esperado.");

                    ArrayAccessorExpression result = new ArrayAccessorExpression(SourceInterval.Merge(operand.Interval, symbol.Interval), operand);
                    Expression indexer = ParseExpression();
                    result.AddIndexer(indexer);

                    while (lexer.NextSymbol(",", false) != null)
                    {
                        indexer = ParseExpression();
                        result.AddIndexer(indexer);
                    }

                    lexer.NextSymbol("]");
                    return result;
                }
            }

            lexer.PreviusToken();
            return operand;
        }

        private Expression ParseUnaryExpression()
        {
            Symbol symbol = lexer.NextSymbol(false);
            if (symbol == null)
                return ParsePostFixExpression();

            Expression operand;
            switch (symbol.Value)
            {
                case "+":
                    return ParsePostFixExpression();

                case "-":
                    operand = ParsePostFixExpression();
                    return new UnaryExpression(SourceInterval.Merge(symbol.Interval, operand.Interval), UnaryOperation.NEGATION, operand);

                case "*":
                    operand = ParsePostFixExpression();
                    return new UnaryExpression(SourceInterval.Merge(symbol.Interval, operand.Interval), UnaryOperation.POINTER_INDIRECTION, operand);

                case "!":
                    operand = ParsePostFixExpression();
                    return new UnaryExpression(SourceInterval.Merge(symbol.Interval, operand.Interval), UnaryOperation.LOGICAL_NOT, operand);

                case "~":
                    operand = ParsePostFixExpression();
                    return new UnaryExpression(SourceInterval.Merge(symbol.Interval, operand.Interval), UnaryOperation.BITWISE_NOT, operand);

                case "++":
                    operand = ParsePostFixExpression();
                    return new UnaryExpression(SourceInterval.Merge(symbol.Interval, operand.Interval), UnaryOperation.PRE_INCREMENT, operand);

                case "--":
                    operand = ParsePostFixExpression();
                    return new UnaryExpression(SourceInterval.Merge(symbol.Interval, operand.Interval), UnaryOperation.PRE_DECREMENT, operand);

                case "&":
                    operand = ParsePostFixExpression();
                    return new UnaryExpression(SourceInterval.Merge(symbol.Interval, operand.Interval), UnaryOperation.POINTER_TO, operand);
            }

            lexer.PreviusToken();
            return ParsePostFixExpression();
        }
        private Expression ParseCastExpression()
        {
            if (lexer.NextKeyword("cast", false) == null)
                return ParseUnaryExpression();

            Symbol start = lexer.NextSymbol("<");
            AbstractType type = ParseType();
            lexer.NextSymbol(">");
            lexer.NextSymbol("(");
            Expression operand = ParseExpression();
            Symbol end = lexer.NextSymbol(")");

            return new CastExpression(SourceInterval.Merge(start.Interval, end.Interval), type, operand);
        }

        private Expression ParseMultiplicativeExpression()
        {
            Expression leftOperand = ParseCastExpression();
            while (true)
            {
                Symbol symbol = lexer.NextSymbol(false);
                if (symbol == null)
                    return leftOperand;

                Expression rightOperand;
                switch (symbol.Value)
                {
                    case "*":
                        rightOperand = ParseCastExpression();
                        leftOperand = new BinaryExpression(SourceInterval.Merge(leftOperand.Interval, rightOperand.Interval), BinaryOperation.MUL, leftOperand, rightOperand);
                        break;

                    case "/":
                        rightOperand = ParseCastExpression();
                        leftOperand = new BinaryExpression(SourceInterval.Merge(leftOperand.Interval, rightOperand.Interval), BinaryOperation.DIV, leftOperand, rightOperand);
                        break;

                    case "%":
                        rightOperand = ParseCastExpression();
                        leftOperand = new BinaryExpression(SourceInterval.Merge(leftOperand.Interval, rightOperand.Interval), BinaryOperation.MOD, leftOperand, rightOperand);
                        break;

                    default:
                        lexer.PreviusToken();
                        return leftOperand;
                }
            }
        }

        private Expression ParseAdditiveExpression()
        {
            Expression leftOperand = ParseMultiplicativeExpression();
            while (true)
            {
                Symbol symbol = lexer.NextSymbol(false);
                if (symbol == null)
                    return leftOperand;

                Expression rightOperand;
                switch (symbol.Value)
                {
                    case "+":
                        rightOperand = ParseMultiplicativeExpression();
                        leftOperand = new BinaryExpression(SourceInterval.Merge(leftOperand.Interval, rightOperand.Interval), BinaryOperation.ADD, leftOperand, rightOperand);
                        break;

                    case "-":
                        rightOperand = ParseMultiplicativeExpression();
                        leftOperand = new BinaryExpression(SourceInterval.Merge(leftOperand.Interval, rightOperand.Interval), BinaryOperation.SUB, leftOperand, rightOperand);
                        break;

                    default:
                        lexer.PreviusToken();
                        return leftOperand;
                }
            }
        }

        private Expression ParseShiftExpression()
        {
            Expression leftOperand = ParseAdditiveExpression();

            Symbol symbol = lexer.NextSymbol(false);
            if (symbol == null)
                return leftOperand;

            Expression rightOperand;
            switch (symbol.Value)
            {
                case "<<":
                    rightOperand = ParseAdditiveExpression();
                    return new BinaryExpression(SourceInterval.Merge(leftOperand.Interval, rightOperand.Interval), BinaryOperation.SHIFT_LEFT, leftOperand, rightOperand);

                case ">>":
                    rightOperand = ParseAdditiveExpression();
                    return new BinaryExpression(SourceInterval.Merge(leftOperand.Interval, rightOperand.Interval), BinaryOperation.SHIFT_RIGHT, leftOperand, rightOperand);

                case ">>>":
                    rightOperand = ParseAdditiveExpression();
                    return new BinaryExpression(SourceInterval.Merge(leftOperand.Interval, rightOperand.Interval), BinaryOperation.UNSIGNED_SHIFT_RIGHT, leftOperand, rightOperand);
            }

            lexer.PreviusToken();
            return leftOperand;
        }

        private Expression ParseInequalityExpression()
        {
            Expression leftOperand = ParseShiftExpression();

            Symbol symbol = lexer.NextSymbol(false);
            if (symbol == null)
                return leftOperand;

            Expression rightOperand;
            switch (symbol.Value)
            {
                case ">":
                    rightOperand = ParseShiftExpression();
                    return new BinaryExpression(SourceInterval.Merge(leftOperand.Interval, rightOperand.Interval), BinaryOperation.GREATER, leftOperand, rightOperand);

                case ">=":
                    rightOperand = ParseShiftExpression();
                    return new BinaryExpression(SourceInterval.Merge(leftOperand.Interval, rightOperand.Interval), BinaryOperation.GREATER_OR_EQUALS, leftOperand, rightOperand);

                case "<":
                    rightOperand = ParseShiftExpression();
                    return new BinaryExpression(SourceInterval.Merge(leftOperand.Interval, rightOperand.Interval), BinaryOperation.LESS, leftOperand, rightOperand);

                case "<=":
                    rightOperand = ParseShiftExpression();
                    return new BinaryExpression(SourceInterval.Merge(leftOperand.Interval, rightOperand.Interval), BinaryOperation.LESS_OR_EQUALS, leftOperand, rightOperand);
            }

            lexer.PreviusToken();
            return leftOperand;
        }

        private Expression ParseEqualityExpression()
        {
            Expression leftOperand = ParseInequalityExpression();

            Symbol symbol = lexer.NextSymbol(false);
            if (symbol == null)
                return leftOperand;

            Expression rightOperand;
            switch (symbol.Value)
            {
                case "==":
                    rightOperand = ParseInequalityExpression();
                    return new BinaryExpression(SourceInterval.Merge(leftOperand.Interval, rightOperand.Interval), BinaryOperation.EQUALS, leftOperand, rightOperand);

                case "!=":
                    rightOperand = ParseInequalityExpression();
                    return new BinaryExpression(SourceInterval.Merge(leftOperand.Interval, rightOperand.Interval), BinaryOperation.NOT_EQUALS, leftOperand, rightOperand);
            }

            lexer.PreviusToken();
            return leftOperand;
        }

        private Expression ParseBitwiseAndExpression()
        {
            Expression leftOperand = ParseEqualityExpression();
            while (true)
            {                
                if (lexer.NextSymbol("&", false) == null)
                    return leftOperand;

                Expression rightOperand = ParseEqualityExpression();
                leftOperand = new BinaryExpression(SourceInterval.Merge(leftOperand.Interval, rightOperand.Interval), BinaryOperation.BITWISE_AND, leftOperand, rightOperand);
            }
        }

        private Expression ParseBitwiseXorExpression()
        {
            Expression leftOperand = ParseBitwiseAndExpression();
            while (true)
            {
                if (lexer.NextSymbol("|", false) == null)
                    return leftOperand;

                Expression rightOperand = ParseBitwiseAndExpression();
                leftOperand = new BinaryExpression(SourceInterval.Merge(leftOperand.Interval, rightOperand.Interval), BinaryOperation.BITWISE_AND, leftOperand, rightOperand);
            }
        }

        private Expression ParseBitwiseOrExpression()
        {
            Expression leftOperand = ParseBitwiseXorExpression();
            while (true)
            {
                if (lexer.NextSymbol("|", false) == null)
                    return leftOperand;

                Expression rightOperand = ParseBitwiseXorExpression();
                leftOperand = new BinaryExpression(SourceInterval.Merge(leftOperand.Interval, rightOperand.Interval), BinaryOperation.BITWISE_OR, leftOperand, rightOperand);
            }
        }

        private Expression ParseLogicalAndExpression()
        {
            Expression leftOperand = ParseBitwiseOrExpression();
            while (true)
            {
                if (lexer.NextSymbol("&&", false) == null)
                    return leftOperand;

                Expression rightOperand = ParseBitwiseOrExpression();
                leftOperand = new BinaryExpression(SourceInterval.Merge(leftOperand.Interval, rightOperand.Interval), BinaryOperation.LOGICAL_AND, leftOperand, rightOperand);
            }
        }

        private Expression ParseLogicalXorExpression()
        {
            Expression leftOperand = ParseLogicalAndExpression();
            while (true)
            {
                if (lexer.NextSymbol("^^", false) == null)
                    return leftOperand;

                Expression rightOperand = ParseLogicalAndExpression();
                leftOperand = new BinaryExpression(SourceInterval.Merge(leftOperand.Interval, rightOperand.Interval), BinaryOperation.LOGICAL_XOR, leftOperand, rightOperand);
            }
        }

        private Expression ParseLogicalOrExpression()
        {
            Expression leftOperand = ParseLogicalXorExpression();
            while (true)
            {
                if (lexer.NextSymbol("||", false) == null)
                    return leftOperand;

                Expression rightOperand = ParseLogicalXorExpression();
                leftOperand = new BinaryExpression(SourceInterval.Merge(leftOperand.Interval, rightOperand.Interval), BinaryOperation.LOGICAL_OR, leftOperand, rightOperand);
            }
        }

        private Expression ParseExpression()
        {
            Expression leftOperand = ParseLogicalOrExpression();

            Symbol symbol = lexer.NextSymbol(false);
            if (symbol == null)
                return leftOperand;

            Expression rightOperand;
            switch (symbol.Value)
            {
                case "=":
                    rightOperand = ParseExpression();
                    return new BinaryExpression(SourceInterval.Merge(leftOperand.Interval, rightOperand.Interval), BinaryOperation.STORE, leftOperand, rightOperand);

                case "+=":
                    rightOperand = ParseExpression();
                    return new BinaryExpression(SourceInterval.Merge(leftOperand.Interval, rightOperand.Interval), BinaryOperation.STORE_ADD, leftOperand, rightOperand);

                case "-=":
                    rightOperand = ParseExpression();
                    return new BinaryExpression(SourceInterval.Merge(leftOperand.Interval, rightOperand.Interval), BinaryOperation.STORE_SUB, leftOperand, rightOperand);

                case "*=":
                    rightOperand = ParseExpression();
                    return new BinaryExpression(SourceInterval.Merge(leftOperand.Interval, rightOperand.Interval), BinaryOperation.STORE_MUL, leftOperand, rightOperand);

                case "/=":
                    rightOperand = ParseExpression();
                    return new BinaryExpression(SourceInterval.Merge(leftOperand.Interval, rightOperand.Interval), BinaryOperation.STORE_DIV, leftOperand, rightOperand);

                case "%=":
                    rightOperand = ParseExpression();
                    return new BinaryExpression(SourceInterval.Merge(leftOperand.Interval, rightOperand.Interval), BinaryOperation.STORE_MOD, leftOperand, rightOperand);

                case "&=":
                    rightOperand = ParseExpression();
                    return new BinaryExpression(SourceInterval.Merge(leftOperand.Interval, rightOperand.Interval), BinaryOperation.STORE_AND, leftOperand, rightOperand);
                case "|=":
                    rightOperand = ParseExpression();
                    return new BinaryExpression(SourceInterval.Merge(leftOperand.Interval, rightOperand.Interval), BinaryOperation.STORE_OR, leftOperand, rightOperand);

                case "^=":
                    rightOperand = ParseExpression();
                    return new BinaryExpression(SourceInterval.Merge(leftOperand.Interval, rightOperand.Interval), BinaryOperation.STORE_XOR, leftOperand, rightOperand);

                case "<<=":
                    rightOperand = ParseExpression();
                    return new BinaryExpression(SourceInterval.Merge(leftOperand.Interval, rightOperand.Interval), BinaryOperation.STORE_SHIFT_LEFT, leftOperand, rightOperand);

                case ">>=":
                    rightOperand = ParseExpression();
                    return new BinaryExpression(SourceInterval.Merge(leftOperand.Interval, rightOperand.Interval), BinaryOperation.STORE_SHIFT_RIGHT, leftOperand, rightOperand);

                case ">>>=":
                    rightOperand = ParseExpression();
                    return new BinaryExpression(SourceInterval.Merge(leftOperand.Interval, rightOperand.Interval), BinaryOperation.STORE_UNSIGNED_SHIFT_RIGHT, leftOperand, rightOperand);
            }

            lexer.PreviusToken();
            return leftOperand;
        }

        public AbstractType ParseType()
        {
            AbstractType result = null;

            if (lexer.NextSymbol("*", false) != null)
            {
                AbstractType type = ParseType();
                return new PointerType(type);
            }

            Keyword kw = lexer.NextKeyword(false);
            if (kw != null)
            {
                switch (kw.Value)
                {
                    case "void":
                        result = PrimitiveType.VOID;
                        break;

                    case "bool":
                        result = PrimitiveType.BOOL;
                        break;

                    case "byte":
                        result = PrimitiveType.BYTE;
                        break;

                    case "char":
                        result = PrimitiveType.CHAR;
                        break;

                    case "short":
                        result = PrimitiveType.SHORT;
                        break;

                    case "int":
                        result = PrimitiveType.INT;
                        break;

                    case "long":
                        result = PrimitiveType.LONG;
                        break;

                    case "float":
                        result = PrimitiveType.FLOAT;
                        break;

                    case "real":
                        result = PrimitiveType.DOUBLE;
                        break;

                    default:
                        lexer.PreviusToken();
                        break;
                }
            }

            if (result == null)
            {
                Identifier id = lexer.NextIdentifier();
                result = FindStruct(id.Name);
                if (result == null)
                    throw new CompilerException(id.Interval, "Tipo não declarado: '" + id.Name + "'");
            }

            while (true)
            {
                if (lexer.NextSymbol("[", false) != null)
                {
                    if (PrimitiveType.IsPrimitiveVoid(result))
                        throw new CompilerException(kw.Interval, "Uso inválido do tipo void.");

                    ArrayType a = new ArrayType(result);

                    NumericLiteral number = lexer.NextNumber();
                    if (!(number is IntLiteral n))
                        throw new CompilerException(number.Interval, "Literal inteiro esperado.");

                    if (n.Value <= 0)
                        throw new CompilerException(number.Interval, "Tamanho de array deve ser maior que zero.");

                    a.AddBoundary(n.Value);

                    while (lexer.NextSymbol(",", false) != null)
                    {
                        number = lexer.NextNumber();
                        if (!(number is IntLiteral n2))
                            throw new CompilerException(number.Interval, "Literal inteiro esperado.");

                        if (n2.Value <= 0)
                            throw new CompilerException(number.Interval, "Tamanho de array deve ser maior que zero.");

                        a.AddBoundary(n2.Value);
                    }

                    lexer.NextSymbol("]");

                    result = a;
                }
                else
                {
                    if (PrimitiveType.IsPrimitiveVoid(result))
                        throw new CompilerException(kw.Interval, "Uso inválido do tipo void.");

                    return result;
                }
            }
        }

        public void ParseParamsDeclaration(Function function)
        {
            while (true)
            {
                bool byRef = lexer.NextSymbol("&", false) != null;

                Identifier id = lexer.NextIdentifier();
                lexer.NextSymbol(":");
                AbstractType type = ParseType();

                Parameter p = function.DeclareParameter(id.Name, type, byRef);
                if (p == null)
                    throw new CompilerException(id.Interval, "Parâmetro '" + id.Name + "' já declarado.");

                if (lexer.NextSymbol(",", false) == null)
                    break;
            }

            function.ComputeParametersOffsets();
        }

        public void ParseFieldsDeclaration(StructType st)
        {
            while (true)
            {
                Identifier id = lexer.NextIdentifier();
                lexer.NextSymbol(":");
                AbstractType type = ParseType();

                Field field = st.DeclareField(id.Name, type);
                if (field == null)
                    throw new CompilerException(id.Interval, "Campo '" + id.Name + "' já declarado.");

                lexer.NextSymbol(";");

                if (lexer.NextSymbol("}", false) != null)
                    return;
            }
        }

        private void CompilePop(Assembler assembler, AbstractType type)
        {
            if (!PrimitiveType.IsPrimitiveVoid(type))
                assembler.EmitSubSP(type.Size());
        }

        private Statement ParseStatement()
        {
            if (lexer.NextSymbol(";", false) != null)
                return new EmptyStatement(lexer.CurrentInterval(lexer.CurrentPos));

            if (lexer.NextSymbol("{", false) != null)
            {
                BlockStatement result = ParseBlock();
                return result;
            }

            Keyword kw = lexer.NextKeyword(false);
            if (kw != null)
            {
                switch (kw.Value)
                {
                    case "var":
                    {
                        DeclarationStatement result = ParseVariableDeclaration();
                        lexer.NextSymbol(";");
                        return result;
                    }

                    case "se":
                    {
                        lexer.NextSymbol("(");

                        Expression expression = ParseExpression();
                        
                        Symbol end = lexer.NextSymbol(")");
            
                        Statement thenStatement = ParseStatement();

                        Statement elseStatement = null;
                        if (lexer.NextKeyword("senão", false) != null)
                            elseStatement = ParseStatement();

                        IfStatement result = new IfStatement(SourceInterval.Merge(kw.Interval, end.Interval), expression, thenStatement, elseStatement);
                        return result;
                    }

                    case "para":
                    {
                        lexer.NextSymbol("(");

                        ForStatement result = new ForStatement(kw.Interval);

                        // inicializadores
                        if (lexer.NextSymbol(";", false) == null)
                        {
                            do
                            {
                                Expression initializer = ParseExpression();
                                result.AddInitializer(initializer);
                            }
                            while (lexer.NextSymbol(",", false) != null);

                            lexer.NextSymbol(";");
                        }

                        // expressão de controle
                        if (lexer.NextSymbol(";", false) == null)
                        {
                            Expression expression = ParseExpression();
                            result.Expression = expression;

                            lexer.NextSymbol(";");
                        }

                        // atualizadores
                        if (lexer.NextSymbol(")", false) == null)
                        {
                            do
                            {
                                Expression updater = ParseExpression();
                                result.AddUpdater(updater);
                            }
                            while (lexer.NextSymbol(",", false) != null);

                            lexer.NextSymbol(")");
                        }

                        Statement stateent = ParseStatement();
                        result.Statement = stateent;

                        return result;
                    }

                    case "enquanto":
                    {
                        lexer.NextSymbol("(");

                        Expression expression = ParseExpression();

                        Symbol end = lexer.NextSymbol(")");

                        Statement statement = ParseStatement();

                        return new WhileStatement(SourceInterval.Merge(kw.Interval, end.Interval), expression, statement);
                    }

                    case "repita":
                    {
                        Statement statement = ParseStatement();

                        lexer.NextKeyword("enquanto");
                        lexer.NextSymbol("(");

                        Expression expression = ParseExpression();

                        Symbol end = lexer.NextSymbol(")");

                        return new DoStatement(SourceInterval.Merge(kw.Interval, end.Interval), expression, statement);
                    }

                    case "leia":
                    {
                        ReadStatement result = new ReadStatement(kw.Interval);

                        if (lexer.NextSymbol(";", false) != null)
                            throw new CompilerException(lexer.CurrentInterval(lexer.CurrentPos), "Expressão esperada.");

                        Expression expression = ParseExpression();
                        result.AddExpression(expression);

                        while (lexer.NextSymbol(",", false) != null)
                        {
                            expression = ParseExpression();
                            result.AddExpression(expression);
                        }

                        lexer.NextSymbol(";");

                        return result;
                    }

                    case "escreva":
                    case "escrevaln":
                    {
                        PrintStatement result = new PrintStatement(kw.Interval, kw.Value == "escrevaln");

                        if (lexer.NextSymbol(";", false) != null)
                            throw new CompilerException(lexer.CurrentInterval(lexer.CurrentPos), "Expressão esperada.");

                        Expression expression = ParseExpression();
                        result.AddExpression(expression);

                        while (lexer.NextSymbol(",", false) != null)
                        {
                            expression = ParseExpression();
                            result.AddExpression(expression);
                        }

                        lexer.NextSymbol(";");

                        return result;
                    }

                    case "retorne":
                    {
                        Expression expression = null;

                        Symbol end;
                        if ((end = lexer.NextSymbol(";", false)) == null)
                        {
                            expression = ParseExpression();
                            end = lexer.NextSymbol(";");
                        }

                        return new ReturnStatement(SourceInterval.Merge(kw.Interval, end.Interval), expression);
                    }

                    case "quebra":
                    {
                        Symbol end = lexer.NextSymbol(";");

                        return new BreakStatement(SourceInterval.Merge(kw.Interval, end.Interval));
                    }
                }

                lexer.PreviusToken();
            }

            Expression expr = ParseExpression();
            lexer.NextSymbol(";");
            return new ExpressionStatement(expr.Interval, expr);
        }

        private void CompileArrayIndexer(Function function, Context context, Assembler assembler, Expression indexer)
        {
            AbstractType indexerType = CompileExpression(function, context, assembler, indexer);

            if (indexerType is PrimitiveType pt)
            {
                switch (pt.Primitive)
                {
                    case Primitive.BOOL:
                    case Primitive.CHAR:
                    case Primitive.LONG:
                    case Primitive.FLOAT:
                    case Primitive.DOUBLE:
                        throw new CompilerException(indexer.Interval, "Tipo de indexador inválido: '" + pt + "'.");
                }
            }
        }

        private AbstractType CompileAssignableExpression(Function function, Context context, Assembler assembler, Expression expression, out Variable storeVar)
        {
            if (expression is UnaryExpression u)
            {
                Expression operand = u.Operand;
                AbstractType operandType = CompileExpression(function, context, assembler, operand);
                if (u.Operation != UnaryOperation.POINTER_INDIRECTION)
                    throw new CompilerException(operand.Interval, "A expressão do lado esquerdo não é atribuível.");

                if (!(operandType is PointerType ptr))
                    throw new CompilerException(operand.Interval, "Indireção de ponteiros só pode ser feita com um tipo 'pointer'.");

                storeVar = null;
                return ptr.Type;
            }

            if (expression is FieldAcessorExpression f)
            {
                Expression operand = f.Operand;
                string fieldName = f.Field;

                AbstractType operandType = CompileAssignableExpression(function, context, assembler, operand, out _);

                if (operandType is StructType s)
                {
                    Field field = s.FindField(fieldName);
                    if (field == null)
                        throw new CompilerException(expression.Interval, "Campo '" + fieldName + "' não encontrado na estrutura: '" + s.Name + "'.");

                    assembler.EmitLoadConst(field.Offset);
                    assembler.EmitAdd();

                    storeVar = null;
                    return field.Type;
                }

                if (operandType is PointerType ptr)
                {
                    if (ptr.Type == null || !(ptr.Type is StructType s2))
                        throw new CompilerException(operand.Interval, "Pointeiro de estrutura esperado.");

                    assembler.EmitLoadStack32();

                    Field field = s2.FindField(fieldName);
                    if (field == null)
                        throw new CompilerException(expression.Interval, "Campo '" + fieldName + "' não encontrado na estrutura: '" + s2.Name + "'.");

                    assembler.EmitLoadConst(field.Offset);
                    assembler.EmitAdd();

                    storeVar = null;
                    return field.Type;
                }

                throw new CompilerException(operand.Interval, "Acesso de membros em um tipo que não é estrutura: '" + operandType + "'.");
            }

            if (expression is ArrayAccessorExpression a)
            {
                Expression operand = a.Operand;               
                AbstractType operandType = CompileAssignableExpression(function, context, assembler, operand, out _);

                if (operandType is ArrayType at)
                {
                    int rank = at.Rank;

                    if (a.IndexerCount == 0)
                        throw new CompilerException(operand.Interval, "Não foi fornecido nenhum índice para o array.");

                    if (a.IndexerCount != rank)
                        throw new CompilerException(operand.Interval, "Número de inídices fornecidos é diferente da dimensão do array.");

                    Expression indexer = a[0];
                    CompileArrayIndexer(function, context, assembler, indexer);

                    for (int i = 1; i < rank; i++)
                    {
                        indexer = a[i];
                        assembler.EmitLoadConst(at[i]);
                        assembler.EmitMul();
                        CompileArrayIndexer(function, context, assembler, indexer);
                        assembler.EmitAdd();
                    }

                    assembler.EmitLoadConst(at.Type.Size());
                    assembler.EmitMul();
                    assembler.EmitAdd();

                    storeVar = null;
                    return at.Type;
                }

                if (operandType is PointerType ptr)
                {
                    if (ptr.Type == null)
                        throw new CompilerException(operand.Interval, "Não é possível realizar esta operação em um ponteiro do tipo void.");

                    if (a.IndexerCount == 0)
                        throw new CompilerException(operand.Interval, "Não foi fornecido nenhum índice para o ponteiro.");

                    if (a.IndexerCount != 1)
                        throw new CompilerException(operand.Interval, "Deve-se fornecer somente um índice para o ponteiro.");

                    assembler.EmitLoadStack32();

                    Expression indexer = a[0];
                    CompileArrayIndexer(function, context, assembler, indexer);
                    assembler.EmitLoadConst(ptr.Type.Size());
                    assembler.EmitMul();
                    assembler.EmitAdd();

                    storeVar = null;
                    return ptr.Type;
                }

                throw new CompilerException(operand.Interval, "Tipo '" + operandType + "' não é um array.");
            }

            if (expression is PrimaryExpression p)
            {
                if (p.PrimaryType != PrimaryType.IDENTIFIER)
                    throw new CompilerException(expression.Interval, "Tipo de expressão não atribuível.");

                IdentifierExpression id = (IdentifierExpression) p;
                string name = id.Name;

                bool byRef = false;
                Variable var = context.FindVariable(id.Name);
                if (var == null)
                {
                    var = FindGlobalVariable(id.Name);
                    if (var == null)
                        throw new CompilerException(id.Interval, "Identificador'" + id.Name + "' não declarado.");

                    // variável local ou parâmetro
                    int offset = var.Offset;
                    assembler.EmitLoadConst(offset);
                }
                else
                {
                    // variável local ou parâmetro
                    int offset = var.Offset;
                    
                    if (var is Parameter param && param.ByRef)
                    {
                        byRef = true;
                        assembler.EmitLoadLocal32(offset);
                    }
                    else
                        assembler.EmitLoadLocalAddress(offset);
                }

                storeVar = !byRef && (var.Type is PrimitiveType || var.Type is PointerType) ? var : null;
                return var.Type;
            }

            throw new CompilerException(expression.Interval, "Tipo de expressão não atribuível.");
        }

        private void CompileLoad(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BOOL:
                    case Primitive.BYTE:
                        assembler.EmitLoadStack8();
                        return;

                    case Primitive.CHAR:
                    case Primitive.SHORT:
                        assembler.EmitLoadStack16();
                        return;

                    case Primitive.INT:
                    case Primitive.FLOAT:
                        assembler.EmitLoadStack32();
                        return;

                    case Primitive.LONG:
                    case Primitive.DOUBLE:
                        assembler.EmitLoadStack64();
                        return;
                }
            }

            if (type is StructType s)
            {
                // TODO Implementar
                return;
            }

            if (type is ArrayType a)
            {
                // TODO Implementar
                return;
            }

            if (type is PointerType)
            {
                assembler.EmitLoadStack32();
                return;
            }

            throw new CompilerException(interval, "Tipo desconhecido: '" + type + "'.");
        }

        private void CompileLoad(Assembler assembler, Variable loadVar, SourceInterval interval)
        {
            AbstractType type = loadVar.Type;
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BOOL:
                    case Primitive.BYTE:
                        if (loadVar is GlobalVariable)
                            assembler.EmitLoadGlobal8(loadVar.Offset);
                        else
                        {
                            assembler.EmitLoadLocal32(loadVar.Offset);
                            if (loadVar is Parameter param && param.ByRef)
                                assembler.EmitLoadStack8();
                        }

                        return;

                    case Primitive.CHAR:
                    case Primitive.SHORT:
                        if (loadVar is GlobalVariable)
                            assembler.EmitLoadGlobal16(loadVar.Offset);
                        else
                        {
                            assembler.EmitLoadLocal32(loadVar.Offset);
                            if (loadVar is Parameter param && param.ByRef)
                                assembler.EmitLoadStack16();
                        }

                        return;

                    case Primitive.INT:
                    case Primitive.FLOAT:
                        if (loadVar is GlobalVariable)
                            assembler.EmitLoadGlobal32(loadVar.Offset);
                        else
                        {
                            assembler.EmitLoadLocal32(loadVar.Offset);
                            if (loadVar is Parameter param && param.ByRef)
                                assembler.EmitLoadStack32();
                        }

                        return;

                    case Primitive.LONG:
                    case Primitive.DOUBLE:
                        if (loadVar is GlobalVariable)
                            assembler.EmitLoadGlobal64(loadVar.Offset);
                        else if (loadVar is Parameter param && param.ByRef)
                        {
                            assembler.EmitLoadLocal32(loadVar.Offset);
                            assembler.EmitLoadStack32();
                        }
                        else
                            assembler.EmitLoadLocal64(loadVar.Offset);

                        return;
                }
            }

            if (type is PointerType)
            {
                if (loadVar is GlobalVariable)
                    assembler.EmitLoadGlobal32(loadVar.Offset);
                else
                {
                    assembler.EmitLoadLocal32(loadVar.Offset);
                    if (loadVar is Parameter param && param.ByRef)
                        assembler.EmitLoadStack32();
                }

                return;
            }

            throw new CompilerException(interval, "Tipo desconhecido: '" + type + "'.");
        }

        private void CompileStore(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BOOL:
                    case Primitive.BYTE:
                        assembler.EmitStoreStack8();
                        return;

                    case Primitive.CHAR:
                    case Primitive.SHORT:
                        assembler.EmitStoreStack16();
                        return;

                    case Primitive.INT:
                    case Primitive.FLOAT:
                        assembler.EmitStoreStack32();
                        return;

                    case Primitive.LONG:
                    case Primitive.DOUBLE:
                        assembler.EmitStoreStack64();
                        return;
                }
            }

            if (type is StructType s)
            {
                // TODO Implementar
                return;
            }

            if (type is ArrayType a)
            {
                // TODO Implementar
                return;
            }

            if (type is PointerType)
            {
                assembler.EmitStoreStack32();
                return;
            }

            throw new CompilerException(interval, "Tipo desconhecido: '" + type + "'.");
        }

        private void CompileStoreGlobal(Assembler assembler, AbstractType type, int offset, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BOOL:
                    case Primitive.BYTE:
                        assembler.EmitStoreGlobal8(offset);
                        return;

                    case Primitive.CHAR:
                    case Primitive.SHORT:
                        assembler.EmitStoreGlobal16(offset);
                        return;

                    case Primitive.INT:
                    case Primitive.FLOAT:
                        assembler.EmitStoreGlobal32(offset);
                        return;

                    case Primitive.LONG:
                    case Primitive.DOUBLE:
                        assembler.EmitStoreGlobal64(offset);
                        return;
                }
            }

            if (type is PointerType)
            {
                assembler.EmitStoreGlobal32(offset);
                return;
            }

            throw new CompilerException(interval, "Tipo desconhecido: '" + type + "'.");
        }

        private void CompileStoreLocal(Assembler assembler, AbstractType type, int offset, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BOOL:
                    case Primitive.BYTE:
                        assembler.EmitStoreLocal8(offset);
                        return;

                    case Primitive.CHAR:
                    case Primitive.SHORT:
                        assembler.EmitStoreLocal16(offset);
                        return;

                    case Primitive.INT:
                    case Primitive.FLOAT:
                        assembler.EmitStoreLocal32(offset);
                        return;

                    case Primitive.LONG:
                    case Primitive.DOUBLE:
                        assembler.EmitStoreLocal64(offset);
                        return;
                }
            }

            if (type is PointerType)
            {
                assembler.EmitStoreLocal32(offset);
                return;
            }

            throw new CompilerException(interval, "Tipo desconhecido: '" + type + "'.");
        }

        private void CompileStore(Assembler assembler, Variable storeVar, SourceInterval interval)
        {
            if (storeVar is GlobalVariable)
                CompileStoreGlobal(assembler, storeVar.Type, storeVar.Offset, interval);
            else
                CompileStoreLocal(assembler, storeVar.Type, storeVar.Offset, interval);
        }

        private void CompileStoreAdd(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitAdd();
                        assembler.EmitStoreStack8();
                        break;

                    case Primitive.SHORT:
                        assembler.EmitAdd();
                        assembler.EmitStoreStack16();
                        break;

                    case Primitive.INT:
                        assembler.EmitAdd();
                        assembler.EmitStoreStack32();
                        return;

                    case Primitive.LONG:
                        assembler.EmitAdd64();
                        assembler.EmitStoreStack64();
                        return;

                    case Primitive.FLOAT:
                        assembler.EmitFAdd();
                        assembler.EmitStoreStack32();
                        return;

                    case Primitive.DOUBLE:
                        assembler.EmitFAdd64();
                        assembler.EmitStoreStack64();
                        return;
                }
            }

            if (type is PointerType)
            {
                assembler.EmitAdd();
                assembler.EmitStoreStack32();
                return;
            }

            throw new CompilerException(interval, "Operação inválida para o tipo '" + type + "'.");
        }

        private void CompileStoreAdd(Assembler assembler, Variable storeVar, SourceInterval interval)
        {
            AbstractType type = storeVar.Type;
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitAdd();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal8(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal8(storeVar.Offset);

                        return;

                    case Primitive.SHORT:
                        assembler.EmitAdd();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal16(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal16(storeVar.Offset);

                        return;

                    case Primitive.INT:
                        assembler.EmitAdd();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal32(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal32(storeVar.Offset);

                        return;

                    case Primitive.LONG:
                        assembler.EmitAdd64();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal64(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal64(storeVar.Offset);

                        return;

                    case Primitive.FLOAT:
                        assembler.EmitFAdd();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal32(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal32(storeVar.Offset);

                        return;

                    case Primitive.DOUBLE:
                        assembler.EmitFAdd64();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal64(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal64(storeVar.Offset);

                        return;
                }
            }

            if (type is PointerType)
            {
                assembler.EmitAdd();

                if (storeVar is GlobalVariable)
                    assembler.EmitStoreGlobal32(storeVar.Offset);
                else
                    assembler.EmitStoreLocal32(storeVar.Offset);

                return;
            }

            throw new CompilerException(interval, "Operação inválida para o tipo '" + type + "'.");
        }

        private void CompileStoreSub(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitSub();
                        assembler.EmitStoreStack8();
                        return;

                    case Primitive.SHORT:
                        assembler.EmitSub();
                        assembler.EmitStoreStack16();
                        return;

                    case Primitive.INT:
                        assembler.EmitSub();
                        assembler.EmitStoreStack32();
                        return;

                    case Primitive.LONG:
                        assembler.EmitSub64();
                        assembler.EmitStoreStack64();
                        return;

                    case Primitive.FLOAT:
                        assembler.EmitFSub();
                        assembler.EmitStoreStack32();
                        return;

                    case Primitive.DOUBLE:
                        assembler.EmitFSub64();
                        assembler.EmitStoreStack64();
                        return;
                }
            }

            if (type is PointerType)
            {
                assembler.EmitSub();
                assembler.EmitStoreStack32();
                return;
            }

            throw new CompilerException(interval, "Operação inválida para o tipo '" + type + "'.");
        }

        private void CompileStoreSub(Assembler assembler, Variable storeVar, SourceInterval interval)
        {
            AbstractType type = storeVar.Type;
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitSub();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal8(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal8(storeVar.Offset);

                        return;

                    case Primitive.SHORT:
                        assembler.EmitSub();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal16(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal16(storeVar.Offset);

                        return;

                    case Primitive.INT:
                        assembler.EmitSub();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal32(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal32(storeVar.Offset);

                        return;

                    case Primitive.LONG:
                        assembler.EmitSub64();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal64(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal64(storeVar.Offset);

                        return;

                    case Primitive.FLOAT:
                        assembler.EmitFSub();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal32(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal32(storeVar.Offset);

                        return;

                    case Primitive.DOUBLE:
                        assembler.EmitFSub64();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal64(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal64(storeVar.Offset);

                        return;
                }
            }

            if (type is PointerType)
            {
                assembler.EmitSub();

                if (storeVar is GlobalVariable)
                    assembler.EmitStoreGlobal32(storeVar.Offset);
                else
                    assembler.EmitStoreLocal32(storeVar.Offset);

                return;
            }

            throw new CompilerException(interval, "Operação inválida para o tipo '" + type + "'.");
        }

        private void CompileStoreMul(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitMul();
                        assembler.EmitStoreStack8();
                        return;

                    case Primitive.SHORT:
                        assembler.EmitMul();
                        assembler.EmitStoreStack16();
                        return;

                    case Primitive.INT:
                        assembler.EmitMul();
                        assembler.EmitStoreStack32();
                        return;

                    case Primitive.LONG:
                        assembler.EmitMul64();
                        assembler.EmitStoreStack64();
                        return;

                    case Primitive.FLOAT:
                        assembler.EmitFMul();
                        assembler.EmitStoreStack32();
                        return;

                    case Primitive.DOUBLE:
                        assembler.EmitFMul64();
                        assembler.EmitStoreStack64();
                        return;
                }
            }

            throw new CompilerException(interval, "Operação inválida para o tipo '" + type + "'.");
        }

        private void CompileStoreMul(Assembler assembler, Variable storeVar, SourceInterval interval)
        {
            AbstractType type = storeVar.Type;
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitMul();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal8(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal8(storeVar.Offset);

                        return;

                    case Primitive.SHORT:
                        assembler.EmitMul();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal16(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal16(storeVar.Offset);

                        return;

                    case Primitive.INT:
                        assembler.EmitMul();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal32(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal32(storeVar.Offset);

                        return;

                    case Primitive.LONG:
                        assembler.EmitMul64();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal64(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal64(storeVar.Offset);

                        return;

                    case Primitive.FLOAT:
                        assembler.EmitFMul();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal32(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal32(storeVar.Offset);

                        return;

                    case Primitive.DOUBLE:
                        assembler.EmitFMul64();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal64(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal64(storeVar.Offset);

                        return;
                }
            }

            throw new CompilerException(interval, "Operação inválida para o tipo '" + type + "'.");
        }

        private void CompileStoreDiv(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitDiv();
                        assembler.EmitStoreStack8();
                        return;

                    case Primitive.SHORT:
                        assembler.EmitDiv();
                        assembler.EmitStoreStack16();

                        return;
                    case Primitive.INT:
                        assembler.EmitDiv();
                        assembler.EmitStoreStack32();
                        return;

                    case Primitive.LONG:
                        assembler.EmitDiv64();
                        assembler.EmitStoreStack64();
                        return;

                    case Primitive.FLOAT:
                        assembler.EmitFDiv();
                        assembler.EmitStoreStack32();
                        return;

                    case Primitive.DOUBLE:
                        assembler.EmitFDiv64();
                        assembler.EmitStoreStack64();
                        return;
                }
            }

            throw new CompilerException(interval, "Operação inválida para o tipo '" + type + "'.");
        }

        private void CompileStoreDiv(Assembler assembler, Variable storeVar, SourceInterval interval)
        {
            AbstractType type = storeVar.Type;
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitDiv();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal8(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal8(storeVar.Offset);

                        return;

                    case Primitive.SHORT:
                        assembler.EmitDiv();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal16(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal16(storeVar.Offset);


                        return;
                    case Primitive.INT:
                        assembler.EmitDiv();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal32(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal32(storeVar.Offset);

                        return;

                    case Primitive.LONG:
                        assembler.EmitDiv64();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal64(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal64(storeVar.Offset);

                        return;

                    case Primitive.FLOAT:
                        assembler.EmitFDiv();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal32(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal32(storeVar.Offset);

                        return;

                    case Primitive.DOUBLE:
                        assembler.EmitFDiv64();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal64(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal64(storeVar.Offset);

                        return;
                }
            }

            throw new CompilerException(interval, "Operação inválida para o tipo '" + type + "'.");
        }

        private void CompileStoreMod(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitMod();
                        assembler.EmitStoreStack8();
                        return;

                    case Primitive.SHORT:
                        assembler.EmitMod();
                        assembler.EmitStoreStack16();
                        return;

                    case Primitive.INT:
                        assembler.EmitMod();
                        assembler.EmitStoreStack32();
                        return;

                    case Primitive.LONG:
                        assembler.EmitMod64();
                        assembler.EmitStoreStack64();
                        return;
                }
            }

            throw new CompilerException(interval, "Operação inválida para o tipo '" + type + "'.");
        }

        private void CompileStoreMod(Assembler assembler, Variable storeVar, SourceInterval interval)
        {
            AbstractType type = storeVar.Type;
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitMod();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal8(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal8(storeVar.Offset);

                        return;

                    case Primitive.SHORT:
                        assembler.EmitMod();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal16(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal16(storeVar.Offset);

                        return;

                    case Primitive.INT:
                        assembler.EmitMod();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal32(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal32(storeVar.Offset);

                        return;

                    case Primitive.LONG:
                        assembler.EmitMod64();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal64(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal64(storeVar.Offset);

                        return;
                }
            }

            throw new CompilerException(interval, "Operação inválida para o tipo '" + type + "'.");
        }

        private void CompileStoreAnd(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BOOL:
                    case Primitive.BYTE:
                        assembler.EmitAnd();
                        assembler.EmitStoreStack8();
                        return;

                    case Primitive.SHORT:
                        assembler.EmitAnd();
                        assembler.EmitStoreStack16();
                        return;

                    case Primitive.INT:
                        assembler.EmitAnd();
                        assembler.EmitStoreStack32();
                        return;

                    case Primitive.LONG:
                        assembler.EmitAnd64();
                        assembler.EmitStoreStack64();
                        return;
                }
            }

            throw new CompilerException(interval, "Operação inválida para o tipo '" + type + "'.");
        }

        private void CompileStoreAnd(Assembler assembler, Variable storeVar, SourceInterval interval)
        {
            AbstractType type = storeVar.Type;
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BOOL:
                    case Primitive.BYTE:
                        assembler.EmitAnd();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal8(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal8(storeVar.Offset);

                        return;

                    case Primitive.SHORT:
                        assembler.EmitAnd();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal16(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal16(storeVar.Offset);

                        return;

                    case Primitive.INT:
                        assembler.EmitAnd();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal32(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal32(storeVar.Offset);

                        return;

                    case Primitive.LONG:
                        assembler.EmitAnd64();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal64(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal64(storeVar.Offset);

                        return;
                }
            }

            throw new CompilerException(interval, "Operação inválida para o tipo '" + type + "'.");
        }

        private void CompileStoreOr(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BOOL:
                    case Primitive.BYTE:
                        assembler.EmitOr();
                        assembler.EmitStoreStack8();
                        return;

                    case Primitive.SHORT:
                        assembler.EmitOr();
                        assembler.EmitStoreStack16();

                        return;
                    case Primitive.INT:
                        assembler.EmitOr();
                        assembler.EmitStoreStack32();
                        return;

                    case Primitive.LONG:
                        assembler.EmitOr64();
                        assembler.EmitStoreStack64();
                        return;
                }
            }

            throw new CompilerException(interval, "Operação inválida para o tipo '" + type + "'.");
        }

        private void CompileStoreOr(Assembler assembler, Variable storeVar, SourceInterval interval)
        {
            AbstractType type = storeVar.Type;
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BOOL:
                    case Primitive.BYTE:
                        assembler.EmitOr();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal8(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal8(storeVar.Offset);

                        return;

                    case Primitive.SHORT:
                        assembler.EmitOr();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal16(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal16(storeVar.Offset);

                        return;

                    case Primitive.INT:
                        assembler.EmitOr();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal32(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal32(storeVar.Offset);

                        return;

                    case Primitive.LONG:
                        assembler.EmitOr64();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal64(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal64(storeVar.Offset);

                        return;
                }
            }

            throw new CompilerException(interval, "Operação inválida para o tipo '" + type + "'.");
        }

        private void CompileStoreXor(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BOOL:
                    case Primitive.BYTE:
                        assembler.EmitXor();
                        assembler.EmitStoreStack8();
                        return;

                    case Primitive.SHORT:
                        assembler.EmitXor();
                        assembler.EmitStoreStack16();
                        return;

                    case Primitive.INT:
                        assembler.EmitXor();
                        assembler.EmitStoreStack32();
                        return;

                    case Primitive.LONG:
                        assembler.EmitXor64();
                        assembler.EmitStoreStack64();
                        return;
                }
            }

            throw new CompilerException(interval, "Operação inválida para o tipo '" + type + "'.");
        }

        private void CompileStoreXor(Assembler assembler, Variable storeVar, SourceInterval interval)
        {
            AbstractType type = storeVar.Type;
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BOOL:
                    case Primitive.BYTE:
                        assembler.EmitXor();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal8(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal8(storeVar.Offset);

                        return;

                    case Primitive.SHORT:
                        assembler.EmitXor();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal16(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal16(storeVar.Offset);


                        return;
                    case Primitive.INT:
                        assembler.EmitXor();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal32(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal32(storeVar.Offset);

                        return;

                    case Primitive.LONG:
                        assembler.EmitXor64();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal64(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal64(storeVar.Offset);

                        return;
                }
            }

            throw new CompilerException(interval, "Operação inválida para o tipo '" + type + "'.");
        }

        private void CompileStoreShiftLeft(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitShl();
                        assembler.EmitStoreStack8();

                        return;
                    case Primitive.SHORT:
                        assembler.EmitShl();
                        assembler.EmitStoreStack16();
                        return;

                    case Primitive.INT:
                        assembler.EmitShl();
                        assembler.EmitStoreStack32();
                        return;

                    case Primitive.LONG:
                        assembler.EmitShl64();
                        assembler.EmitStoreStack64();
                        return;
                }
            }

            throw new CompilerException(interval, "Operação inválida para o tipo '" + type + "'.");
        }

        private void CompileStoreShiftLeft(Assembler assembler, Variable storeVar, SourceInterval interval)
        {
            AbstractType type = storeVar.Type;
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitShl();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal8(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal8(storeVar.Offset);

                        return;

                    case Primitive.SHORT:
                        assembler.EmitShl();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal16(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal16(storeVar.Offset);

                        return;

                    case Primitive.INT:
                        assembler.EmitShl();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal32(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal32(storeVar.Offset);

                        return;

                    case Primitive.LONG:
                        assembler.EmitShl64();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal64(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal64(storeVar.Offset);

                        return;
                }
            }

            throw new CompilerException(interval, "Operação inválida para o tipo '" + type + "'.");
        }

        private void CompileStoreShiftRight(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitShr();
                        assembler.EmitStoreStack8();
                        return;

                    case Primitive.SHORT:
                        assembler.EmitShr();
                        assembler.EmitStoreStack16();
                        return;

                    case Primitive.INT:
                        assembler.EmitShr();
                        assembler.EmitStoreStack32();
                        return;

                    case Primitive.LONG:
                        assembler.EmitShr64();
                        assembler.EmitStoreStack64();
                        return;
                }
            }

            throw new CompilerException(interval, "Operação inválida para o tipo '" + type + "'.");
        }

        private void CompileStoreShiftRight(Assembler assembler, Variable storeVar, SourceInterval interval)
        {
            AbstractType type = storeVar.Type;
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitShr();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal8(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal8(storeVar.Offset);

                        return;

                    case Primitive.SHORT:
                        assembler.EmitShr();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal16(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal16(storeVar.Offset);

                        return;

                    case Primitive.INT:
                        assembler.EmitShr();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal32(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal32(storeVar.Offset);

                        return;

                    case Primitive.LONG:
                        assembler.EmitShr64();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal64(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal64(storeVar.Offset);

                        return;
                }
            }

            throw new CompilerException(interval, "Operação inválida para o tipo '" + type + "'.");
        }

        private void CompileStoreUnsignedShiftRight(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitUShr();
                        assembler.EmitStoreStack8();
                        return;

                    case Primitive.SHORT:
                        assembler.EmitUShr();
                        assembler.EmitStoreStack16();

                        return;
                    case Primitive.INT:
                        assembler.EmitUShr();
                        assembler.EmitStoreStack32();
                        return;

                    case Primitive.LONG:
                        assembler.EmitUShr64();
                        assembler.EmitStoreStack64();
                        return;
                }
            }

            throw new CompilerException(interval, "Operação inválida para o tipo '" + type + "'.");
        }

        private void CompileStoreUnsignedShiftRight(Assembler assembler, Variable storeVar, SourceInterval interval)
        {
            AbstractType type = storeVar.Type;
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitUShr();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal8(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal8(storeVar.Offset);

                        return;

                    case Primitive.SHORT:
                        assembler.EmitUShr();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal16(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal16(storeVar.Offset);

                        return;

                    case Primitive.INT:
                        assembler.EmitUShr();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal32(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal32(storeVar.Offset);

                        return;

                    case Primitive.LONG:
                        assembler.EmitUShr64();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal64(storeVar.Offset);
                        else
                            assembler.EmitStoreLocal64(storeVar.Offset);

                        return;
                }
            }

            throw new CompilerException(interval, "Operação inválida para o tipo '" + type + "'.");
        }

        private void CompileStoreExpression(Function function, Context context, Assembler assembler, BinaryOperation operation, Expression leftOperand, Expression rightOperand)
        {
            AbstractType leftType;
            AbstractType rightType;

            if (operation == BinaryOperation.STORE)
            {
                Assembler leftAssembler = new Assembler();
                leftType = CompileAssignableExpression(function, context, leftAssembler, leftOperand, out Variable storeVar);
                if (storeVar == null)
                    assembler.Emit(leftAssembler);

                rightType = CompileExpression(function, context, assembler, rightOperand);
                CompileCast(assembler, rightType, leftType, false, rightOperand.Interval);

                if (storeVar != null)
                    CompileStore(assembler, storeVar, leftOperand.Interval);
                else
                    CompileStore(assembler, leftType, leftOperand.Interval);

                return;
            }

            Assembler tempAssembler = new Assembler();
            leftType = CompileAssignableExpression(function, context, tempAssembler, leftOperand, out Variable storeVar2);

            if (storeVar2 == null)
            {
                Assembler tempAssembler2 = new Assembler();
                tempAssembler2.Emit(tempAssembler);
                CompileLoad(tempAssembler2, leftType, leftOperand.Interval);
                assembler.Emit(tempAssembler);
                assembler.Emit(tempAssembler2);
            }
            else
                CompileLoad(assembler, storeVar2, leftOperand.Interval);

            rightType = CompileExpression(function, context, assembler, rightOperand);
            CompileCast(assembler, rightType, leftType, false, rightOperand.Interval);

            switch (operation)
            {
                case BinaryOperation.STORE_OR:
                {
                    if (storeVar2 == null)
                        CompileStoreOr(assembler, leftType, leftOperand.Interval);
                    else
                        CompileStoreOr(assembler, storeVar2, leftOperand.Interval);

                    break;
                }

                case BinaryOperation.STORE_XOR:
                {
                    if (storeVar2 == null)
                        CompileStoreXor(assembler, leftType, leftOperand.Interval);
                    else
                        CompileStoreXor(assembler, storeVar2, leftOperand.Interval);

                    break;
                }

                case BinaryOperation.STORE_AND:
                {
                    if (storeVar2 == null)
                        CompileStoreAnd(assembler, leftType, leftOperand.Interval);
                    else
                        CompileStoreAnd(assembler, storeVar2, leftOperand.Interval);

                    break;
                }

                case BinaryOperation.STORE_SHIFT_LEFT:
                {
                    if (storeVar2 == null)
                        CompileStoreShiftLeft(assembler, leftType, leftOperand.Interval);
                    else
                        CompileStoreShiftLeft(assembler, storeVar2, leftOperand.Interval);

                    break;
                }

                case BinaryOperation.STORE_SHIFT_RIGHT:
                {
                    if (storeVar2 == null)
                        CompileStoreShiftRight(assembler, leftType, leftOperand.Interval);
                    else
                        CompileStoreShiftRight(assembler, storeVar2, leftOperand.Interval);

                    break;
                }

                case BinaryOperation.STORE_UNSIGNED_SHIFT_RIGHT:
                {
                    if (storeVar2 == null)
                        CompileStoreUnsignedShiftRight(assembler, leftType, leftOperand.Interval);
                    else
                        CompileStoreUnsignedShiftRight(assembler, storeVar2, leftOperand.Interval);

                    break;
                }

                case BinaryOperation.STORE_ADD:
                {
                    if (storeVar2 == null)
                        CompileStoreAdd(assembler, leftType, leftOperand.Interval);
                    else
                        CompileStoreAdd(assembler, storeVar2, leftOperand.Interval);

                    break;
                }

                case BinaryOperation.STORE_SUB:
                {
                    if (storeVar2 == null)
                        CompileStoreSub(assembler, leftType, leftOperand.Interval);
                    else
                        CompileStoreSub(assembler, storeVar2, leftOperand.Interval);

                    break;
                }

                case BinaryOperation.STORE_MUL:
                {
                    if (storeVar2 == null)
                        CompileStoreMul(assembler, leftType, leftOperand.Interval);
                    else
                        CompileStoreMul(assembler, storeVar2, leftOperand.Interval);

                    break;
                }

                case BinaryOperation.STORE_DIV:
                {
                    if (storeVar2 == null)
                        CompileStoreDiv(assembler, leftType, leftOperand.Interval);
                    else
                        CompileStoreDiv(assembler, storeVar2, leftOperand.Interval);

                    break;
                }

                case BinaryOperation.STORE_MOD:
                {
                    if (storeVar2 == null)
                        CompileStoreMod(assembler, leftType, leftOperand.Interval);
                    else
                        CompileStoreMod(assembler, storeVar2, leftOperand.Interval);

                    break;
                }

                default:
                    throw new CompilerException(leftOperand.Interval, "Operador '" + operation + "' desconhecido.");
            }
        }

        private AbstractType CompileExpression(Function function, Context context, Assembler assembler, Expression expression)
        {
            if (expression is UnaryExpression u)
            {
                Expression operand = u.Operand;
                switch (u.Operation)
                {
                    case UnaryOperation.NEGATION:
                    {
                        AbstractType operandType = CompileExpression(function, context, assembler, operand);
                        if (operandType is PrimitiveType pt)
                        {
                            switch (pt.Primitive)
                            {
                                case Primitive.BOOL:
                                case Primitive.CHAR:
                                    throw new CompilerException(operand.Interval, "Operação não definida para o tipo '" + pt + "'.");

                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitNeg();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitNeg64();
                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitFNeg();
                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitFNeg64();
                                    break;
                            }

                            return operandType;
                        }
                        
                        throw new CompilerException(operand.Interval, "Operação não definida para o tipo '" + operandType + "'.");
                    }

                    case UnaryOperation.BITWISE_NOT:
                    {
                        AbstractType operandType = CompileExpression(function, context, assembler, operand);
                        if (operandType is PrimitiveType pt)
                        {
                            switch (pt.Primitive)
                            {
                                case Primitive.BOOL:
                                case Primitive.CHAR:
                                case Primitive.FLOAT:
                                case Primitive.DOUBLE:
                                    throw new CompilerException(operand.Interval, "Operação não definida para o tipo '" + pt + "'.");

                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitNot();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitNot64();
                                    break;
                            }

                            return operandType;
                        }
                        
                        throw new CompilerException(operand.Interval, "Operação não definida para o tipo '" + operandType + "'.");
                    }

                    case UnaryOperation.LOGICAL_NOT:
                    {
                        AbstractType operandType = CompileExpression(function, context, assembler, operand);
                        if (operandType is PrimitiveType pt)
                        {
                            switch (pt.Primitive)
                            {      
                                case Primitive.BOOL:
                                    assembler.EmitNot();
                                    break;

                                default:
                                    throw new CompilerException(operand.Interval, "Operação não definida para o tipo '" + pt + "'.");
                            }

                            return PrimitiveType.BOOL;
                        }

                        throw new CompilerException(operand.Interval, "Operação não definida para o tipo '" + operandType + "'.");
                    }

                    case UnaryOperation.POINTER_INDIRECTION:
                    {
                        AbstractType operandType = CompileExpression(function, context, assembler, operand);
                        if (operandType is PointerType ptr)
                        {
                            AbstractType ptrType = ptr.Type;
                            if (ptrType == null)
                                assembler.EmitLoadStack32();
                            else if (ptrType is PrimitiveType pt)
                            {
                                switch (pt.Primitive)
                                {
                                    case Primitive.VOID:
                                        throw new CompilerException(operand.Interval, "Indireção em ponteiros do tipo void não pode ser feita.");

                                    case Primitive.BOOL:
                                    case Primitive.BYTE:
                                    case Primitive.CHAR:
                                    case Primitive.SHORT:
                                    case Primitive.INT:
                                    case Primitive.FLOAT:
                                        assembler.EmitLoadStack32();
                                        break;

                                    case Primitive.LONG:
                                    case Primitive.DOUBLE:
                                        assembler.EmitLoadStack64();                                
                                        break;
                                }
                            }
                            else if (ptrType is ArrayType at)
                            {
                                // TODO Implementar
                            }
                            else if (ptrType is StructType st)
                            {
                                // TODO Implementar
                            }
                            else if (ptrType is PointerType)
                                assembler.EmitLoadStack32();

                            return ptrType;
                        }
                        
                        throw new CompilerException(operand.Interval, "Operação não definida para o tipo '" + operandType + "'.");
                    }

                    case UnaryOperation.PRE_INCREMENT: // ++x <=> x = x + 1
                    {
                        Assembler tempAssembler = new Assembler();
                        AbstractType operandType = CompileAssignableExpression(function, context, tempAssembler, operand, out Variable storeVar);

                        if (storeVar == null)
                        {
                            assembler.Emit(tempAssembler);

                            assembler.Emit(tempAssembler);
                            CompileLoad(assembler, operandType, operand.Interval);
                        }
                        else
                            CompileLoad(assembler, storeVar, operand.Interval);

                        if (operandType is PrimitiveType pt)
                        {
                            switch (pt.Primitive)
                            {
                                case Primitive.BOOL:
                                case Primitive.CHAR:
                                    throw new CompilerException(operand.Interval, "Operação não definida para o tipo '" + pt + "'.");

                                case Primitive.BYTE:
                                    assembler.EmitLoadConst((byte) 1);
                                    assembler.EmitAdd();

                                    if (storeVar == null)
                                        assembler.EmitStoreStack8();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal8(storeVar.Offset);
                                    else        
                                        assembler.EmitStoreLocal8(storeVar.Offset);

                                    break;

                                case Primitive.SHORT:
                                    assembler.EmitLoadConst((short) 1);
                                    assembler.EmitAdd();

                                    if (storeVar == null)
                                        assembler.EmitStoreStack16();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal16(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal16(storeVar.Offset);

                                    break;

                                case Primitive.INT:
                                    assembler.EmitLoadConst(1);
                                    assembler.EmitAdd();

                                    if (storeVar == null)
                                        assembler.EmitStoreStack32();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal32(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal32(storeVar.Offset);

                                    break;

                                case Primitive.LONG:
                                    assembler.EmitLoadConst(1L);
                                    assembler.EmitAdd64();

                                    if (storeVar == null)
                                        assembler.EmitStoreStack64();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal64(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal64(storeVar.Offset);

                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitLoadConst(1F);
                                    assembler.EmitFAdd();

                                    if (storeVar == null)
                                        assembler.EmitStoreStack32();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal32(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal32(storeVar.Offset);

                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitLoadConst(1.0);
                                    assembler.EmitFAdd64();

                                    if (storeVar == null)
                                        assembler.EmitStoreStack64();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal64(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal64(storeVar.Offset);

                                    break;
                            }

                            if (storeVar == null)
                            {
                                assembler.Emit(tempAssembler);
                                CompileLoad(assembler, operandType, operand.Interval);
                            }
                            else
                                CompileLoad(assembler, storeVar, operand.Interval);

                            return operandType;
                        }
                        
                        throw new CompilerException(operand.Interval, "Operação não definida para o tipo '" + operandType + "'.");
                    }

                    case UnaryOperation.PRE_DECREMENT:
                    {
                        Assembler tempAssembler = new Assembler();
                        AbstractType operandType = CompileAssignableExpression(function, context, tempAssembler, operand, out Variable storeVar);
                        
                        if (storeVar == null)
                        {
                            assembler.Emit(tempAssembler);

                            assembler.Emit(tempAssembler);
                            CompileLoad(assembler, operandType, operand.Interval);
                        }
                        else
                            CompileLoad(assembler, storeVar, operand.Interval);

                        if (operandType is PrimitiveType pt)
                        {
                            switch (pt.Primitive)
                            {
                                case Primitive.BOOL:
                                case Primitive.CHAR:
                                    throw new CompilerException(operand.Interval, "Operação não definida para o tipo '" + pt + "'.");

                                case Primitive.BYTE:
                                    assembler.EmitLoadConst((byte) 1);
                                    assembler.EmitSub();

                                    if (storeVar == null)
                                        assembler.EmitStoreStack8();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal8(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal8(storeVar.Offset);

                                    break;

                                case Primitive.SHORT:
                                    assembler.EmitLoadConst((short) 1);
                                    assembler.EmitSub();

                                    if (storeVar == null)
                                        assembler.EmitStoreStack16();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal16(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal16(storeVar.Offset);

                                    break;

                                case Primitive.INT:
                                    assembler.EmitLoadConst(1);
                                    assembler.EmitSub();

                                    if (storeVar == null)
                                        assembler.EmitStoreStack32();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal32(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal32(storeVar.Offset);

                                    break;

                                case Primitive.LONG:
                                    assembler.EmitLoadConst(1L);
                                    assembler.EmitSub64();

                                    if (storeVar == null)
                                        assembler.EmitStoreStack64();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal64(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal64(storeVar.Offset);

                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitLoadConst(1F);
                                    assembler.EmitFSub();

                                    if (storeVar == null)
                                        assembler.EmitStoreStack32();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal32(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal32(storeVar.Offset);

                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitLoadConst(1.0);
                                    assembler.EmitFSub64();

                                    if (storeVar == null)
                                        assembler.EmitStoreStack64();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal64(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal64(storeVar.Offset);

                                    break;
                            }

                            if (storeVar == null)
                            {
                                assembler.Emit(tempAssembler);
                                CompileLoad(assembler, operandType, operand.Interval);
                            }
                            else
                                CompileLoad(assembler, storeVar, operand.Interval);

                            return operandType;
                        }
                        
                        throw new CompilerException(operand.Interval, "Operação não definida para o tipo '" + operandType + "'.");
                    }

                    case UnaryOperation.POST_INCREMENT:
                    {
                        Assembler tempAssembler = new Assembler();
                        AbstractType operandType = CompileAssignableExpression(function, context, tempAssembler, operand, out Variable storeVar);

                        if (storeVar == null)
                        {
                            Assembler tempAssembler2 = new Assembler();
                            tempAssembler2.Emit(tempAssembler);
                            CompileLoad(tempAssembler2, operandType, operand.Interval);
                            assembler.Emit(tempAssembler2);

                            assembler.Emit(tempAssembler);
                            assembler.Emit(tempAssembler2);
                        }
                        else
                        {
                            Assembler tempAssembler2 = new Assembler();
                            CompileLoad(tempAssembler2, storeVar, operand.Interval);

                            assembler.Emit(tempAssembler2);
                            assembler.Emit(tempAssembler2);
                        }

                        if (operandType is PrimitiveType pt)
                        {
                            switch (pt.Primitive)
                            {
                                case Primitive.BOOL:
                                case Primitive.CHAR:
                                    throw new CompilerException(operand.Interval, "Operação não definida para o tipo '" + pt + "'.");

                                case Primitive.BYTE:
                                    assembler.EmitLoadConst((byte) 1);
                                    assembler.EmitAdd();

                                    if (storeVar == null)
                                        assembler.EmitStoreStack8();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal8(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal8(storeVar.Offset);

                                    break;

                                case Primitive.SHORT:
                                    assembler.EmitLoadConst((short) 1);
                                    assembler.EmitAdd();

                                    if (storeVar == null)
                                        assembler.EmitStoreStack16();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal16(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal16(storeVar.Offset);

                                    break;

                                case Primitive.INT:
                                    assembler.EmitLoadConst(1);
                                    assembler.EmitAdd();

                                    if (storeVar == null)
                                        assembler.EmitStoreStack32();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal32(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal32(storeVar.Offset);

                                    break;

                                case Primitive.LONG:
                                    assembler.EmitLoadConst(1L);
                                    assembler.EmitAdd64();

                                    if (storeVar == null)
                                        assembler.EmitStoreStack64();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal64(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal64(storeVar.Offset);

                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitLoadConst(1F);
                                    assembler.EmitFAdd();

                                    if (storeVar == null)
                                        assembler.EmitStoreStack32();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal32(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal32(storeVar.Offset);

                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitLoadConst(1.0);
                                    assembler.EmitFAdd64();

                                    if (storeVar == null)
                                        assembler.EmitStoreStack64();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal64(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal64(storeVar.Offset);

                                    break;
                            }

                            return operandType;
                        }
                        
                       throw new CompilerException(operand.Interval, "Operação não definida para o tipo '" + operandType + "'.");
                    }

                    case UnaryOperation.POST_DECREMENT:
                    {
                        Assembler tempAssembler = new Assembler();
                        AbstractType operandType = CompileAssignableExpression(function, context, tempAssembler, operand, out Variable storeVar);

                        if (storeVar == null)
                        {
                            Assembler tempAssembler2 = new Assembler();
                            tempAssembler2.Emit(tempAssembler);
                            CompileLoad(tempAssembler2, operandType, operand.Interval);
                            assembler.Emit(tempAssembler2);

                            assembler.Emit(tempAssembler);
                            assembler.Emit(tempAssembler2);
                        }
                        else
                        {
                            Assembler tempAssembler2 = new Assembler();
                            CompileLoad(tempAssembler2, storeVar, operand.Interval);

                            assembler.Emit(tempAssembler2);
                            assembler.Emit(tempAssembler2);
                        }

                        if (operandType is PrimitiveType pt)
                        {
                            switch (pt.Primitive)
                            {
                                case Primitive.BOOL:
                                case Primitive.CHAR:
                                    throw new CompilerException(operand.Interval, "Operação não definida para o tipo '" + pt + "'.");

                                case Primitive.BYTE:
                                    assembler.EmitLoadConst((byte) 1);
                                    assembler.EmitSub();

                                    if (storeVar == null)
                                        assembler.EmitStoreStack8();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal8(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal8(storeVar.Offset);


                                    break;
                                case Primitive.SHORT:
                                    assembler.EmitLoadConst((short) 1);
                                    assembler.EmitSub();

                                    if (storeVar == null)
                                        assembler.EmitStoreStack16();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal16(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal16(storeVar.Offset);

                                    break;

                                case Primitive.INT:
                                    assembler.EmitLoadConst(1);
                                    assembler.EmitSub();

                                    if (storeVar == null)
                                        assembler.EmitStoreStack32();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal32(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal32(storeVar.Offset);

                                    break;

                                case Primitive.LONG:
                                    assembler.EmitLoadConst(1L);
                                    assembler.EmitSub64();

                                    if (storeVar == null)
                                        assembler.EmitStoreStack64();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal64(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal64(storeVar.Offset);

                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitLoadConst(1F);
                                    assembler.EmitFSub();

                                    if (storeVar == null)
                                        assembler.EmitStoreStack32();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal32(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal32(storeVar.Offset);

                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitLoadConst(1.0);
                                    assembler.EmitFSub64();

                                    if (storeVar == null)
                                        assembler.EmitStoreStack64();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal64(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal64(storeVar.Offset);

                                    break;
                            }

                            return operandType;
                        }
                        
                        throw new CompilerException(operand.Interval, "Operação não definida para o tipo '" + operandType + "'.");
                    }

                    case UnaryOperation.POINTER_TO:
                    {
                        Assembler tempAssembler = new Assembler();
                        AbstractType operandType = CompileAssignableExpression(function, context, assembler, operand, out _);

                        if (operandType is ArrayType at)
                            return new PointerType(at.Type);

                        return new PointerType(operandType);
                    }

                    default:
                        throw new CompilerException(expression.Interval, "Operador '" + u.Operation + "' desconhecido.");
                }
            }

            if (expression is BinaryExpression b)
            {
                Expression leftOperand = b.LeftOperand;
                Expression rightOperand = b.RightOperand;

                if (b.Operation <= BinaryOperation.STORE_MOD)
                {
                    CompileStoreExpression(function, context, assembler, b.Operation, leftOperand, rightOperand);
                    return PrimitiveType.VOID;
                }

                Assembler leftAssembler = new Assembler();
                AbstractType leftType = CompileExpression(function, context, leftAssembler, leftOperand);

                Assembler rightAssembler = new Assembler();
                AbstractType rightType = CompileExpression(function, context, rightAssembler, rightOperand);

                switch (b.Operation)
                {
                    case BinaryOperation.LOGICAL_OR:
                    {
                        if (!PrimitiveType.IsPrimitiveBool(leftType))
                            throw new CompilerException(leftOperand.Interval, "Operação não definida entre tipos não booleanos.");

                        if (!PrimitiveType.IsPrimitiveBool(rightType))
                            throw new CompilerException(rightOperand.Interval, "Operação não definida entre tipos não booleanos.");

                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);
                        assembler.EmitOr();
                        return PrimitiveType.BOOL;
                    }

                    case BinaryOperation.LOGICAL_XOR:
                    {
                        if (!PrimitiveType.IsPrimitiveBool(leftType))
                            throw new CompilerException(leftOperand.Interval, "Operação não definida entre tipos não booleanos.");

                        if (!PrimitiveType.IsPrimitiveBool(rightType))
                            throw new CompilerException(rightOperand.Interval, "Operação não definida entre tipos não booleanos.");

                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);
                        assembler.EmitXor();
                        return PrimitiveType.BOOL;
                    }

                    case BinaryOperation.LOGICAL_AND:
                    {
                        if (!PrimitiveType.IsPrimitiveBool(leftType))
                            throw new CompilerException(leftOperand.Interval, "Operação não definida entre tipos não booleanos.");

                        if (!PrimitiveType.IsPrimitiveBool(rightType))
                            throw new CompilerException(rightOperand.Interval, "Operação não definida entre tipos não booleanos.");

                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);
                        assembler.EmitAnd();
                        return PrimitiveType.BOOL;
                    }

                    case BinaryOperation.SHIFT_LEFT:
                    {
                        if (!PrimitiveType.IsUpTo32BitsInt(rightType))
                            throw new CompilerException(rightOperand.Interval, "Tipo inválido para o operando 2: '" + rightType + "'.");

                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);

                        if (PrimitiveType.IsUpTo32BitsInt(leftType))
                            assembler.EmitShl();
                        else if (PrimitiveType.Is64BitsInt(leftType))
                            assembler.EmitShl64();
                        else
                            throw new CompilerException(leftOperand.Interval, "Tipo inválido para o operando 1: '" + leftType + "'.");

                        return leftType;
                    }

                    case BinaryOperation.SHIFT_RIGHT:
                    {
                        if (!PrimitiveType.IsUpTo32BitsInt(rightType))
                            throw new CompilerException(rightOperand.Interval, "Tipo inválido para o operando 2: '" + rightType + "'.");

                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);

                        if (PrimitiveType.IsUpTo32BitsInt(leftType))
                            assembler.EmitShr();
                        else if (PrimitiveType.Is64BitsInt(leftType))
                            assembler.EmitShr64();
                        else
                            throw new CompilerException(leftOperand.Interval, "Tipo inválido para o operando 1: '" + leftType + "'.");

                        return leftType;
                    }

                    case BinaryOperation.UNSIGNED_SHIFT_RIGHT:
                    {
                        if (!PrimitiveType.IsUpTo32BitsInt(rightType))
                            throw new CompilerException(rightOperand.Interval, "Tipo inválido para o operando 2: '" + rightType + "'.");

                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);

                        if (PrimitiveType.IsUpTo32BitsInt(leftType))
                            assembler.EmitUShr();
                        else if (PrimitiveType.Is64BitsInt(leftType))
                            assembler.EmitUShr64();
                        else
                            throw new CompilerException(leftOperand.Interval, "Tipo inválido para o operando 1: '" + leftType + "'.");

                        return leftType;
                    }

                    case BinaryOperation.EQUALS:
                    {
                        if (PrimitiveType.IsPrimitiveBool(leftType) && PrimitiveType.IsPrimitiveBool(rightType))
                        {
                            assembler.Emit(leftAssembler);
                            assembler.EmitLoadConst(1);
                            assembler.EmitAnd();
                            assembler.Emit(rightAssembler);
                            assembler.EmitLoadConst(1);
                            assembler.EmitAnd();
                            assembler.EmitCompareEquals();
                            return PrimitiveType.BOOL;
                        }

                        if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false, leftOperand.Interval);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false, rightOperand.Interval);
                            }

                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);

                            switch (resultType.Primitive)
                            {
                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitCompareEquals();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitCompareEquals64();
                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitFCompareEquals();
                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitFCompareEquals64();
                                    break;
                            }

                            return PrimitiveType.BOOL;
                        }

                        if (PrimitiveType.IsPrimitiveChar(leftType) && PrimitiveType.IsPrimitiveChar(rightType))
                        {
                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);
                            assembler.EmitCompareEquals();
                            return PrimitiveType.BOOL;
                        }

                        if (leftType is PointerType && rightType is PointerType)
                        {
                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);
                            assembler.EmitCompareEquals();
                            return PrimitiveType.BOOL;
                        }

                        if (leftType == rightType)
                        {
                            // TODO Implementar
                            return PrimitiveType.BOOL;
                        }

                        throw new CompilerException(expression.Interval, "Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.NOT_EQUALS:
                    {
                        if (PrimitiveType.IsPrimitiveBool(leftType) && PrimitiveType.IsPrimitiveBool(rightType))
                        {
                            assembler.Emit(leftAssembler);
                            assembler.EmitLoadConst(1);
                            assembler.EmitAnd();
                            assembler.Emit(rightAssembler);
                            assembler.EmitLoadConst(1);
                            assembler.EmitAnd();
                            assembler.EmitCompareNotEquals();
                            return PrimitiveType.BOOL;
                        }

                        if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false, leftOperand.Interval);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false, rightOperand.Interval);
                            }

                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);

                            switch (resultType.Primitive)
                            {
                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitCompareNotEquals();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitCompareNotEquals64();
                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitFCompareNotEquals();
                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitFCompareNotEquals64();
                                    break;
                            }

                            return PrimitiveType.BOOL;
                        }

                        if (PrimitiveType.IsPrimitiveChar(leftType) && PrimitiveType.IsPrimitiveChar(rightType))
                        {
                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);
                            assembler.EmitCompareNotEquals();
                            return PrimitiveType.BOOL;
                        }

                        if (leftType is PointerType && rightType is PointerType)
                        {
                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);
                            assembler.EmitCompareNotEquals();
                            return PrimitiveType.BOOL;
                        }

                        if (leftType == rightType)
                        {
                            // TODO Implementar
                            return PrimitiveType.BOOL;
                        }

                        throw new CompilerException(expression.Interval, "Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.GREATER:
                    {
                        if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false, leftOperand.Interval);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false, rightOperand.Interval);
                            }

                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);

                            switch (resultType.Primitive)
                            {
                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitCompareGreater();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitCompareGreater64();
                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitFCompareGreater();
                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitFCompareGreater64();
                                    break;
                            }

                            return PrimitiveType.BOOL;
                        }

                        if (PrimitiveType.IsPrimitiveChar(leftType) && PrimitiveType.IsPrimitiveChar(rightType))
                        {
                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);
                            assembler.EmitCompareGreater();
                            return PrimitiveType.BOOL;
                        }

                        if (leftType is PointerType && rightType is PointerType)
                        {
                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);
                            assembler.EmitCompareGreater();
                            return PrimitiveType.BOOL;
                        }

                        throw new CompilerException(expression.Interval, "Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.GREATER_OR_EQUALS:
                    {
                        if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false, leftOperand.Interval);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false, rightOperand.Interval);
                            }

                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);

                            switch (resultType.Primitive)
                            {
                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitCompareGreaterOrEquals();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitCompareGreaterOrEquals64();
                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitFCompareGreaterOrEquals();
                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitFCompareGreaterOrEquals64();
                                    break;
                            }

                            return PrimitiveType.BOOL;
                        }

                        if (PrimitiveType.IsPrimitiveChar(leftType) && PrimitiveType.IsPrimitiveChar(rightType))
                        {
                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);
                            assembler.EmitCompareGreaterOrEquals();
                            return PrimitiveType.BOOL;
                        }

                        if (leftType is PointerType && rightType is PointerType)
                        {
                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);
                            assembler.EmitCompareGreaterOrEquals();
                            return PrimitiveType.BOOL;
                        }

                        throw new CompilerException(expression.Interval, "Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.LESS:
                    {
                        if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false, leftOperand.Interval);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false, rightOperand.Interval);
                            }

                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);

                            switch (resultType.Primitive)
                            {
                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitCompareLess();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitCompareLess64();
                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitFCompareLess();
                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitFCompareLess64();
                                    break;
                            }

                            return PrimitiveType.BOOL;
                        }

                        if (PrimitiveType.IsPrimitiveChar(leftType) && PrimitiveType.IsPrimitiveChar(rightType))
                        {
                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);
                            assembler.EmitCompareLess();
                            return PrimitiveType.BOOL;
                        }

                        if (leftType is PointerType && rightType is PointerType)
                        {
                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);
                            assembler.EmitCompareLess();
                            return PrimitiveType.BOOL;
                        }

                        throw new CompilerException(expression.Interval, "Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.LESS_OR_EQUALS:
                    {
                        if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false, leftOperand.Interval);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false, rightOperand.Interval);
                            }

                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);

                            switch (resultType.Primitive)
                            {
                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitCompareLessOrEquals();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitCompareLessOrEquals64();
                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitFCompareLessOrEquals();
                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitFCompareLessOrEquals64();
                                    break;
                            }

                            return PrimitiveType.BOOL;
                        }

                        if (PrimitiveType.IsPrimitiveChar(leftType) && PrimitiveType.IsPrimitiveChar(rightType))
                        {
                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);
                            assembler.EmitCompareLessOrEquals();
                            return PrimitiveType.BOOL;
                        }

                        if (leftType is PointerType && rightType is PointerType)
                        {
                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);
                            assembler.EmitCompareLessOrEquals();
                            return PrimitiveType.BOOL;
                        }

                        throw new CompilerException(expression.Interval, "Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.BITWISE_OR:
                    {
                        if (PrimitiveType.IsPrimitiveInteger(leftType) && PrimitiveType.IsPrimitiveInteger(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false, leftOperand.Interval);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false, rightOperand.Interval);
                            }

                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);

                            switch (resultType.Primitive)
                            {
                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitOr();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitOr64();
                                    break;
                            }

                            return resultType.Size() < 4 ? PrimitiveType.INT : resultType;
                        }

                        throw new CompilerException(expression.Interval, "Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.BITWISE_XOR:
                    {
                        if (PrimitiveType.IsPrimitiveInteger(leftType) && PrimitiveType.IsPrimitiveInteger(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false, leftOperand.Interval);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false, rightOperand.Interval);
                            }

                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);

                            switch (resultType.Primitive)
                            {
                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitXor();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitXor64();
                                    break;
                            }

                            return resultType.Size() < 4 ? PrimitiveType.INT : resultType;
                        }

                        throw new CompilerException(expression.Interval, "Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.BITWISE_AND:
                    {
                        if (PrimitiveType.IsPrimitiveInteger(leftType) && PrimitiveType.IsPrimitiveInteger(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false, leftOperand.Interval);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false, rightOperand.Interval);
                            }

                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);

                            switch (resultType.Primitive)
                            {
                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitAnd();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitAnd64();
                                    break;
                            }

                            return resultType.Size() < 4 ? PrimitiveType.INT : resultType;
                        }

                        throw new CompilerException(expression.Interval, "Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.ADD:
                    {
                        if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false, leftOperand.Interval);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false, rightOperand.Interval);
                            }

                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);

                            switch (resultType.Primitive)
                            {
                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitAdd();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitAdd64();
                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitFAdd();
                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitFAdd64();
                                    break;
                            }

                            return resultType.Size() < 4 ? PrimitiveType.INT : resultType;
                        }

                        if (leftType is PointerType ptr)
                        {
                            if (PrimitiveType.IsPrimitiveVoid(ptr.Type))
                                throw new CompilerException(leftOperand.Interval, "Operação aritimética com ponteiros não permitida para ponteiros do tipo void.");

                            if (!PrimitiveType.IsPrimitiveInteger(rightType))
                                throw new CompilerException(rightOperand.Interval, "Operando direito de uma operação de deslocamento de ponteiros deve ser um inteiro.");

                            int size = ptr.Type.Size();

                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);
                            assembler.EmitLoadConst(size);
                            assembler.EmitMul();
                            assembler.EmitAdd();
                            return leftType;
                        }

                        throw new CompilerException(expression.Interval, "Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.SUB:
                    {
                        if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false, leftOperand.Interval);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false, rightOperand.Interval);
                            }

                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);

                            switch (resultType.Primitive)
                            {
                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitSub();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitSub64();
                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitFSub();
                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitFSub64();
                                    break;
                            }

                            return resultType.Size() < 4 ? PrimitiveType.INT : resultType;
                        }

                        if (leftType is PointerType ptr)
                        {
                            if (PrimitiveType.IsPrimitiveInteger(rightType))
                            {
                                if (PrimitiveType.IsPrimitiveVoid(ptr.Type))
                                    throw new CompilerException(leftOperand.Interval, "Operação aritimética com ponteiros não permitida para ponteiros do tipo void.");

                                int size = ptr.Type.Size();

                                assembler.Emit(leftAssembler);
                                assembler.Emit(rightAssembler);
                                assembler.EmitLoadConst(size);
                                assembler.EmitMul();
                                assembler.EmitSub();
                                return leftType;
                            }

                            if (rightType is PointerType)
                            {
                                assembler.Emit(leftAssembler);
                                assembler.Emit(rightAssembler);
                                assembler.EmitSub();
                                return PrimitiveType.INT;
                            }

                            throw new CompilerException(rightOperand.Interval, "Operando direito de uma operação de subtração envolvendo ponteiros deve ser um inteiro ou um ponteiro.");
                        }

                        throw new CompilerException(expression.Interval, "Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.MUL:
                    {
                        if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false, leftOperand.Interval);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false, rightOperand.Interval);
                            }

                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);

                            switch (resultType.Primitive)
                            {
                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitMul();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitMul64();
                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitFMul();
                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitFMul64();
                                    break;

                                default:
                                    throw new CompilerException(expression.Interval, "Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                            }

                            return resultType.Size() < 4 ? PrimitiveType.INT : resultType;
                        }

                        throw new CompilerException(expression.Interval, "Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.DIV:
                    {
                        if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false, leftOperand.Interval);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false, rightOperand.Interval);
                            }

                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);

                            switch (resultType.Primitive)
                            {
                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitDiv();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitDiv64();
                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitFDiv();
                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitFDiv64();
                                    break;

                                default:
                                    throw new CompilerException(expression.Interval, "Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                            }

                            return resultType.Size() < 4 ? PrimitiveType.INT : resultType;
                        }

                        throw new CompilerException(expression.Interval, "Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.MOD:
                    {
                        if (PrimitiveType.IsPrimitiveInteger(leftType) && PrimitiveType.IsPrimitiveInteger(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false, leftOperand.Interval);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false, rightOperand.Interval);
                            }

                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);

                            switch (resultType.Primitive)
                            {
                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitMod();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitMod64();
                                    break;

                                default:
                                    throw new CompilerException(expression.Interval, "Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                            }

                            return resultType.Size() < 4 ? PrimitiveType.INT : resultType;
                        }

                        throw new CompilerException(expression.Interval, "Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    default:
                        throw new CompilerException(expression.Interval, "Operador '" + b.Operation + "' desconhecido.");
                }
            }

            if (expression is FieldAcessorExpression f)
            {
                Expression operand = f.Operand;
                string fieldName = f.Field;

                AbstractType operandType = CompileAssignableExpression(function, context, assembler, operand, out Variable storeVar);

                if (operandType is StructType s)
                {
                    Field field = s.FindField(fieldName);
                    if (field == null)
                        throw new CompilerException(expression.Interval, "Campo '" + fieldName + "' não encontrado na estrutura: '" + s.Name + "'.");

                    assembler.EmitLoadConst(field.Offset);
                    assembler.EmitAdd();
                    CompileLoad(assembler, field.Type, expression.Interval);

                    return field.Type;
                }

                throw new CompilerException(operand.Interval, "Acesso de membros em um tipo que não é estrutura: '" + operandType + "'.");
            }

            if (expression is ArrayAccessorExpression a)
            {
                Expression operand = a.Operand;
                AbstractType operandType = CompileAssignableExpression(function, context, assembler, operand, out Variable storeVar);

                if (operandType is ArrayType at)
                {
                    int rank = at.Rank;

                    if (a.IndexerCount == 0)
                        throw new CompilerException(operand.Interval, "Não foi fornecido nenhum índice para o array.");

                    if (a.IndexerCount != rank)
                        throw new CompilerException(operand.Interval, "Número de inídices fornecidos é diferente da dimensão do array.");

                    Expression indexer = a[0];
                    CompileArrayIndexer(function, context, assembler, indexer);

                    for (int i = 1; i < rank; i++)
                    {
                        indexer = a[i];
                        assembler.EmitLoadConst(at[i]);
                        assembler.EmitMul();
                        CompileArrayIndexer(function, context, assembler, indexer);
                        assembler.EmitAdd();
                    }

                    assembler.EmitLoadConst(at.Type.Size());
                    assembler.EmitMul();
                    assembler.EmitAdd();

                    CompileLoad(assembler, at.Type, expression.Interval);

                    return at.Type;
                }

                if (operandType is PointerType ptr)
                {
                    if (ptr.Type == null)
                        throw new CompilerException(operand.Interval, "Não é possível realizar essa operação em um ponteiro do tipo void.");

                    if (a.IndexerCount == 0)
                        throw new CompilerException(operand.Interval, "Não foi fornecido nenhum índice para o ponteiro.");

                    if (a.IndexerCount != 1)
                        throw new CompilerException(operand.Interval, "Deve-se fornecer somente um índice único para o ponteiro.");

                    assembler.EmitLoadStack32();

                    Expression indexer = a[0];
                    CompileArrayIndexer(function, context, assembler, indexer);
                    assembler.EmitLoadConst(ptr.Type.Size());
                    assembler.EmitMul();
                    assembler.EmitAdd();

                    CompileLoad(assembler, ptr.Type, expression.Interval);

                    return ptr.Type;
                }

                throw new CompilerException(operand.Interval, "Tipo '" + operandType + "' não é um array.");
            }

            if (expression is PrimaryExpression p)
            {
                switch (p)
                {
                    case BoolLiteralExpression l:
                        assembler.EmitLoadConst(l.Value);
                        return PrimitiveType.BOOL;

                    case ByteLiteralExpression l:
                        assembler.EmitLoadConst(l.Value);
                        return PrimitiveType.BYTE;

                    case CharLiteralExpression l:
                        assembler.EmitLoadConst(l.Value);
                        return PrimitiveType.CHAR;

                    case ShortLiteralExpression l:
                        assembler.EmitLoadConst(l.Value);
                        return PrimitiveType.SHORT;

                    case IntLiteralExpression l:
                        assembler.EmitLoadConst(l.Value);
                        return PrimitiveType.INT;

                    case LongLiteralExpression l:
                        assembler.EmitLoadConst(l.Value);
                        return PrimitiveType.LONG;

                    case FloatLiteralExpression l:
                        assembler.EmitLoadConst(l.Value);
                        return PrimitiveType.FLOAT;

                    case DoubleLiteralExpression l:
                        assembler.EmitLoadConst(l.Value);
                        return PrimitiveType.DOUBLE;

                    case StringLiteralExpression l:
                    {
                        int offset = GetStringOffset(l.Value);
                        assembler.EmitLoadConst(offset);
                        return PointerType.STRING;
                    }

                    case NullLiteralExpression l:
                        assembler.EmitLoadConst(0);
                        return PointerType.NULL;

                    case IdentifierExpression id:
                    {
                        string name = id.Name;

                        Variable var = context.FindVariable(name);
                        if (var == null)
                        {
                            var = FindGlobalVariable(name);
                            if (var == null)
                                throw new CompilerException(id.Interval, "Identificador'" + name + "' não declarado.");
                        }

                        CompileLoad(assembler, var, id.Interval);
                        return var.Type;
                    }
                }

                throw new CompilerException(expression.Interval, "Tipo de expressão primária desconhecido: '" + p + "'");
            }

            if (expression is CallExpression c)
            {
                Expression operand = c.Operand;
                if (operand is IdentifierExpression id)
                {
                    string functionName = id.Name;
                    Function func = FindFunction(id.Name);
                    if (func == null)
                        throw new CompilerException(id.Interval, "Função '" + id.Name + "' não declarada.");

                    AbstractType returnType = func.ReturnType;
                    if (!PrimitiveType.IsPrimitiveVoid(returnType))
                        assembler.EmitAddSP(returnType.Size());

                    if (func.ParamCount != c.ParameterCount)
                        throw new CompilerException(id.Interval, "A quantidade de parâmetros fornecido é diferente da quantidade total de parâmetros esperada.");

                    for (int j = 0; j < func.ParamCount; j++)
                    {
                        Parameter parameter = func[j];
                        Expression expressionParameter = c[j];

                        if (parameter.ByRef)
                        {
                            AbstractType paramType = CompileAssignableExpression(function, context, assembler, expressionParameter, out Variable storeVar);
                            if (paramType != parameter.Type)
                                throw new CompilerException(expressionParameter.Interval, "Parâmetro passado por referência deve ser do mesmo tipo que o parâmetro correspondente da função a ser chamada.");
                        }
                        else
                        {
                            AbstractType paramType = CompileExpression(function, context, assembler, expressionParameter);
                            CompileCast(assembler, paramType, parameter.Type, false, expressionParameter.Interval);
                        }
                    }

                    assembler.EmitCall(func.EntryLabel);

                    return func.ReturnType;
                }

                throw new CompilerException(c.Interval, "Expressão de nome de função inválida.");
            }

            if (expression is CastExpression cs)
            {
                Expression operand = cs.Operand;
                AbstractType type = cs.Type;

                AbstractType operandType = CompileExpression(function, context, assembler, operand);
                CompileCast(assembler, operandType, type, true, operand.Interval);
                return type;
            }

            throw new CompilerException(expression.Interval, "Tipo desconhecido de expressão: " + expression);
        }

        public Variable CheckVariable(Context context, string name, SourceInterval interval)
        {
            Variable var = context.FindVariable(name);
            if (var == null)
            {
                var = FindGlobalVariable(name);
                if (var == null)
                    throw new CompilerException(interval, "Variável '" + name + "' não declarada.");
            }

            return var;
        }

        public void CompileStatement(Function function, Context context, Assembler assembler, Statement statement)
        {
            if (statement is ExpressionStatement e)
            {
                Expression expression = e.Expression;
                AbstractType type = CompileExpression(function, context, assembler, expression);
                CompilePop(assembler, type);
           }
            else if (statement is DeclarationStatement decl)
                CompileVariableDeclaration(function, context, assembler, decl);
            else if (statement is ReturnStatement r)
            {
                Expression expr = r.Expression;

                if (expr != null && PrimitiveType.IsPrimitiveVoid(function.ReturnType))
                    throw new CompilerException(expr.Interval, "A função não possui tipo de retorno.");

                if (expr == null && PrimitiveType.IsPrimitiveVoid(function.ReturnType))
                    throw new CompilerException(r.Interval, "Expressão de retorno esperada.");

                if (expr != null)
                {
                    if (function.ReturnType is PrimitiveType || function.ReturnType is PointerType)
                    {
                        AbstractType returnType = CompileExpression(function, context, assembler, expr);
                        CompileCast(assembler, returnType, function.ReturnType, false, expr.Interval);
                        CompileStoreLocal(assembler, function.ReturnType, function.ReturnOffset, expr.Interval);
                    }
                    else
                    {
                        assembler.EmitLoadLocalAddress(function.ReturnOffset);
                        AbstractType returnType = CompileExpression(function, context, assembler, expr);
                        CompileCast(assembler, returnType, function.ReturnType, false, expr.Interval);
                        CompileStore(assembler, function.ReturnType, expr.Interval);
                    }
                }

                assembler.EmitJump(function.ReturnLabel);
            }
            else if (statement is BreakStatement b)
            {
                Label breakLabel = context.FindNearestBreakLabel();
                if (breakLabel == null)
                    throw new CompilerException(b.Interval, "Instrução 'quebra' deve estar dentro de um loop.");

                lexer.NextSymbol(";");

                assembler.EmitJump(breakLabel);
            }
            else if (statement is ReadStatement rd)
            {
                for (int j = 0; j < rd.ExpressionCount; j++)
                {
                    Expression expr = rd[j];
                    AbstractType exprType = CompileAssignableExpression(function, context, assembler, expr, out Variable storeVar);

                    if (exprType is PrimitiveType p)
                    {
                        switch (p.Primitive)
                        {
                            case Primitive.BOOL:
                                assembler.EmitScanB();
                                break;

                            case Primitive.BYTE:
                                assembler.EmitScan8();
                                break;

                            case Primitive.CHAR:
                                assembler.EmitScanC();
                                break;

                            case Primitive.SHORT:
                                assembler.EmitScan16();
                                break;

                            case Primitive.INT:
                                assembler.EmitScan32();
                                break;

                            case Primitive.LONG:
                                assembler.EmitScan64();
                                break;

                            case Primitive.FLOAT:
                                assembler.EmitFScan();
                                break;

                            case Primitive.DOUBLE:
                                assembler.EmitFScan64();
                                break;

                            default:
                                throw new CompilerException(expr.Interval, "Expressão de tipo primitivo ou string esperada.");
                        }
                    }
                    else if (exprType is PointerType ptr && ptr.IsString)
                        assembler.EmitPScan();
                    else
                        throw new CompilerException(expr.Interval, "Expressão de tipo primitivo ou string esperada.");
                }
            }
            else if (statement is PrintStatement p)
            {
                for (int j = 0; j < p.ExpressionCount; j++)
                {
                    Expression expr = p[j];

                    AbstractType exprType = CompileExpression(function, context, assembler, expr);

                    if (exprType is PrimitiveType pt)
                    {
                        switch (pt.Primitive)
                        {
                            case Primitive.BOOL:
                            {
                                int falseOffset = GetStringOffset("falso");
                                int trueOffset = GetStringOffset("verdadeiro");

                                Label lblFalse = CreateLabel();
                                Label lblEnd = CreateLabel();
                                assembler.EmitJumpIfFalse(lblFalse);
                                assembler.EmitLoadConst(trueOffset);
                                assembler.EmitJump(lblEnd);
                                assembler.BindLabel(lblFalse);
                                assembler.EmitLoadConst(falseOffset);
                                assembler.BindLabel(lblEnd);
                                assembler.EmitPPrint();
                                break;
                            }    

                            case Primitive.BYTE:
                                assembler.EmitPrint32();
                                break;

                            case Primitive.CHAR:
                                assembler.EmitLoadConst((short) 0);
                                assembler.EmitLoadSP();
                                assembler.EmitLoadConst(8);
                                assembler.EmitSub();
                                assembler.EmitPPrint();
                                assembler.EmitSubSP(4);
                                break;

                            case Primitive.SHORT:
                            case Primitive.INT:
                                assembler.EmitPrint32();
                                break;

                            case Primitive.LONG:
                                assembler.EmitPrint64();
                                break;

                            case Primitive.FLOAT:
                                assembler.EmitFPrint();
                                break;

                            case Primitive.DOUBLE:
                                assembler.EmitFPrint64();
                                break;

                            default:
                                throw new CompilerException(expr.Interval, "Expressão de tipo primitivo ou string esperada.");
                        }
                    }
                    else if (exprType is PointerType ptr && ptr.IsString)
                        assembler.EmitPPrint();
                    else
                        throw new CompilerException(expr.Interval, "Expressão de tipo primitivo ou string esperada.");
                }

                if (p.LineBreak)
                {
                    int lineBreakOffset = GetStringOffset("\n");
                    assembler.EmitLoadConst(lineBreakOffset);
                    assembler.EmitPPrint();
                }
            }
            else if (statement is IfStatement i)
            {
                Expression expression = i.Expression;
                Statement thenStatement = i.ThenStatement;
                Statement elseStatement = i.ElseStatement;

                AbstractType exprType = CompileExpression(function, context, assembler, expression);
  
                if (!PrimitiveType.IsPrimitiveBool(exprType))
                    throw new CompilerException(expression.Interval, "Expressão do tipo bool experada.");

                Label lblElse = CreateLabel();
                assembler.EmitJumpIfFalse(lblElse);

                CompileStatement(function, context, assembler, thenStatement);

                Label lblEnd = CreateLabel();
                assembler.EmitJump(lblEnd);

                assembler.BindLabel(lblElse);
                if (elseStatement != null)
                    CompileStatement(function, context, assembler, elseStatement);

                assembler.BindLabel(lblEnd);
            }
            else if (statement is WhileStatement w)
            {
                Expression expression = w.Expression;
                Statement stm = w.Statement;

                Label lblLoop = CreateLabel();
                assembler.BindLabel(lblLoop);

                AbstractType exprType = CompileExpression(function, context, assembler, expression);

                Label lblEnd = CreateLabel();
                context.PushBreakLabel(lblEnd);

                assembler.EmitJumpIfFalse(lblEnd);

                if (!PrimitiveType.IsPrimitiveBool(exprType))
                    throw new CompilerException(expression.Interval, "Expressão do tipo bool experada.");

                CompileStatement(function, context, assembler, stm);

                assembler.EmitJump(lblLoop);
                assembler.BindLabel(lblEnd);

                context.DropBreakLabel();
            }
            else if (statement is DoStatement d)
            {
                Statement stm = d.Statement;
                Expression expr = d.Expression;

                Label lblLoop = CreateLabel();
                assembler.BindLabel(lblLoop);

                Label lblEnd = CreateLabel();
                context.PushBreakLabel(lblEnd);

                CompileStatement(function, context, assembler, stm);

                AbstractType exprType = CompileExpression(function, context, assembler, expr);
                if (!PrimitiveType.IsPrimitiveBool(exprType))
                    throw new CompilerException(expr.Interval, "Expressão do tipo bool experada.");

                assembler.EmitJumpIfFalse(lblLoop);

                assembler.BindLabel(lblEnd);
                context.DropBreakLabel();
            }
            else if (statement is ForStatement f)
            {
                // inicializadores
                for (int j = 0; j < f.InitializerCount; j++)
                {
                    Expression initializer = f.GetInitializer(j);
                    AbstractType initializerType = CompileExpression(function, context, assembler, initializer);
                    CompilePop(assembler, initializerType);
                }

                Label lblLoop = CreateLabel();
                assembler.BindLabel(lblLoop);

                // expressão de controle
                Expression expression = f.Expression;
                if (expression != null)
                {
                    AbstractType expressionType = CompileExpression(function, context, assembler, expression);
                    if (!PrimitiveType.IsPrimitiveBool(expressionType))
                        throw new CompilerException(expression.Interval, "Expressão do tipo bool esperada.");
                }
                else
                    assembler.EmitLoadConst(true);

                Label lblEnd = CreateLabel();
                context.PushBreakLabel(lblEnd);

                assembler.EmitJumpIfFalse(lblEnd);

                Statement stm = f.Statement;
                CompileStatement(function, context, assembler, stm);

                // atualizadores
                for (int j = 0; j < f.UpdaterCount; j++)
                {
                    Expression updater = f.GetUpdater(j);
                    AbstractType updaterType = CompileExpression(function, context, assembler, updater);
                    CompilePop(assembler, updaterType);
                }

                assembler.EmitJump(lblLoop);
                assembler.BindLabel(lblEnd);

                context.DropBreakLabel();
            }
            else if (statement is BlockStatement bl)
            {
                Context newContext = new Context(function, context);
                for (int j = 0; j < bl.StatementCount; j++)
                {
                    Statement stm = bl[j];
                    CompileStatement(function, newContext, assembler, stm);
                }
            }
            else
                throw new CompilerException(statement.Interval, "Tipo desconhecido de statement: " + statement);
        }

        public BlockStatement ParseBlock()
        {
            BlockStatement result = new BlockStatement(lexer.CurrentInterval(lexer.CurrentPos));
            while (lexer.NextSymbol("}", false) == null)
            {
                Statement statement = ParseStatement();
                result.AddStatement(statement);
            }

            return result;
        }

        private DeclarationStatement ParseVariableDeclaration()
        {
            Identifier id = lexer.NextIdentifier();
            lexer.NextSymbol(":");
            AbstractType type = ParseType();

            Expression initializer = null;
            if (lexer.NextSymbol("=", false) != null)
                initializer = ParseExpression();

            DeclarationStatement result = new DeclarationStatement(id.Interval, type);
            result.AddVariable(id.Name, initializer);
            return result;
        }

        public void CompileVariableDeclaration(Function function, Context context, Assembler assembler, DeclarationStatement statement)
        {
            AbstractType type = statement.Type;
            for (int i = 0; i < statement.VariableCount; i++)
            {
                Tuple<string, Expression> tuple = statement[i];
                string name = tuple.Item1;
                Expression initializer = tuple.Item2;

                Variable var = function == null ? DeclareGlobalVariable(name, type) : context.DeclareLocalVariable(function, name, type);
                if (var == null)
                    throw new CompilerException(statement.Interval, "Variável '" + name + "' já declarada.");

                if (initializer != null)
                {
                    if (function == null)
                        throw new CompilerException(statement.Interval, "Variável global não pode ser inicializada.");

                    bool useVar = false;
                    if (var is GlobalVariable)
                        assembler.EmitLoadConst(var.Offset);
                    else
                    {
                        useVar = var.Type is PrimitiveType || var.Type is PointerType;
                        if (!useVar)
                            assembler.EmitLoadLocalAddress(var.Offset);
                    }

                    AbstractType initializerType = CompileExpression(function, context, assembler, initializer);
                    CompileCast(assembler, initializerType, type, false, initializer.Interval);

                    if (useVar)
                        CompileStore(assembler, var, initializer.Interval);
                    else
                        CompileStore(assembler, type, initializer.Interval);
                }
            }
        }

        public void ParseFunctionDeclaration()
        {
            Identifier id = lexer.NextIdentifier();
            Function f = DeclareFunction(id.Name);
            if (f == null)
                throw new CompilerException(id.Interval, "Função '" + id.Name + "' já declarada.");

            lexer.NextSymbol("(");
            if (lexer.NextSymbol(")", false) == null)
            {
                ParseParamsDeclaration(f);
                lexer.NextSymbol(")");
            }

            if (lexer.NextSymbol(":", false) != null)
            {
                AbstractType type = ParseType();
                f.ReturnType = type;
            }

            lexer.NextSymbol("{");

            f.CreateEntryLabel();
            f.CreateReturnLabel();
            f.Block = ParseBlock();
        }

        public void ParseStructDeclaration()
        {
            Identifier id = lexer.NextIdentifier();
            StructType st = DeclareStruct(id.Name);
            if (st == null)
                throw new CompilerException(id.Interval, "Estrutura '" + id.Name + "' já declarada.");

            lexer.NextSymbol("{");
            if (lexer.NextSymbol("}", false) == null)
                ParseFieldsDeclaration(st);
        }

        public bool ParseDeclaration()
        {
            Keyword kw = lexer.NextKeyword(false);
            if (kw != null)
            {
                switch (kw.Value)
                {
                    case "var":
                    {
                        DeclarationStatement declaration = ParseVariableDeclaration();
                        CompileVariableDeclaration(null, null, null, declaration);
                        lexer.NextSymbol(";");
                        return true;
                    }

                    case "função":
                        ParseFunctionDeclaration();
                        return true;

                    case "estrutura":
                        ParseStructDeclaration();
                        return true;
                }

                lexer.PreviusToken();
            }

            Symbol start = lexer.NextSymbol("{");

            Function f = DeclareFunction("@main");
            if (f == null)
                throw new CompilerException(start.Interval, "Ponto de entrada já declarado.");

            entryPoint = f;

            f.CreateEntryLabel();
            f.CreateReturnLabel();
            f.Block = ParseBlock();

            return false;
        }

        public void CompileFunction(Function function, Assembler assembler)
        {
            Context context = new Context(function);
            Assembler tempAssembler = new Assembler();

            CompileStatement(function, context, tempAssembler, function.Block);
            function.BindReturnLabel(tempAssembler);

            function.BindEntryLabel(assembler);
            function.BeginBlock(assembler);
            assembler.Emit(tempAssembler);
            function.EndBlock(assembler);
        }

        public void ParseProgram()
        {
            lexer.NextKeyword("programa");
            lexer.NextIdentifier();
            lexer.NextSymbol("{");
           
            while (ParseDeclaration())
            {
            }

            lexer.NextSymbol("}");

            Token token = lexer.NextToken();
            if (token != null)
                throw new CompilerException(token.Interval, "Fim do programa esperado mas " + token + " encontrado.");
        }

        public AbstractType CompileAdditiveExpression(string expression, Assembler assembler)
        {
            globalVariableOffset = 1;
            entryPoint = null;

            globals.Clear();
            globalTable.Clear();
            structs.Clear();
            structTable.Clear();
            functions.Clear();
            functionTable.Clear();
            labels.Clear();
            stringTable.Clear();

            lexer.Input = expression;

            Context context = new Context(null);
            Expression expr = ParseAdditiveExpression();

            Token token = lexer.NextToken();
            if (token != null)
                throw new CompilerException(token.Interval, "Fim da expressão esperado mas " + token + " encontrado.");

            AbstractType type = CompileExpression(null, context, assembler, expr);

            return type;
        }

        public void CompileProgram(string source, Assembler assembler)
        {
            globalVariableOffset = 1;
            entryPoint = null;

            globals.Clear();
            globalTable.Clear();
            structs.Clear();
            structTable.Clear();
            functions.Clear();
            functionTable.Clear();
            labels.Clear();
            stringTable.Clear();

            lexer.Input = source;

            ParseProgram();

            if (entryPoint != null)
                assembler.EmitCall(entryPoint.EntryLabel);

            assembler.EmitHalt();

            for (int i = 0; i < functions.Count; i++)
            {
                Function f = functions[i];
                CompileFunction(f, assembler);
            }

            assembler.ReserveConstantBuffer(globalVariableOffset);
            foreach (var kv in stringTable)
                assembler.WriteConstant(kv.Value, kv.Key);

            for (int i = 0; i < labels.Count; i++)
            {
                Label label = labels[i];
                if (label.BindedIP != -1)
                    label.UpdateReferences(assembler);
            }
        }
    }
}