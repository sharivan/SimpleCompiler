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
        private List<StructType> structs;
        private List<Function> functions;
        private List<Label> labels;

        private int globalVariableOffset;
        private Function entryPoint;

        public Compiler()
        {
            lexer = new Lexer();
            globals = new List<GlobalVariable>();
            structs = new List<StructType>();
            functions = new List<Function>();
            labels = new List<Label>();

            globalVariableOffset = 0;
            entryPoint = null;
        }

        public Label CreateLabel()
        {
            Label result = new Label();
            labels.Add(result);
            return result;
        }

        public GlobalVariable FindGlobalVariable(string name)
        {
            for (int i = 0; i < globals.Count; i++)
            {
                GlobalVariable var = globals[i];
                if (var.Name == name)
                    return var;
            }

            return null;
        }

        public GlobalVariable DeclareGlobalVariable(string name, AbstractType type)
        {
            GlobalVariable result = FindGlobalVariable(name);
            if (result != null)
                return null;

            result = new GlobalVariable(name, type, globalVariableOffset);
            globalVariableOffset += GetSizeInDWords(type.Size());
            globals.Add(result);
            return result;
        }

        public StructType FindStruct(string name)
        {
            for (int i = 0; i < structs.Count; i++)
            {
                StructType st = structs[i];
                if (st.Name == name)
                    return st;
            }

            return null;
        }

        public StructType DeclareStruct(string name)
        {
            StructType result = FindStruct(name);
            if (result != null)
                return null;

            result = new StructType(name);
            structs.Add(result);
            return result;
        }

        public Function FindFunction(string name)
        {
            for (int i = 0; i < functions.Count; i++)
            {
                Function f = functions[i];
                if (f.Name == name)
                    return f;
            }

            return null;
        }

        public Function DeclareFunction(string name)
        {
            Function result = FindFunction(name);
            if (result != null)
                return null;

            result = new Function(this, name);
            functions.Add(result);
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

        private void CompileCast(Assembler assembler, AbstractType fromType, AbstractType toType, bool isExplicit)
        {
            if (fromType is PrimitiveType p)
            {
                if (toType is PrimitiveType tp)
                {
                    switch (p.Primitive)
                    {
                        case Primitive.BOOL:
                            if (isExplicit ? false : tp.Primitive != Primitive.BOOL)
                                throw new ParserException("O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            // TODO Implementar
                            break;

                        case Primitive.BYTE:
                            if (isExplicit ? tp.Primitive == Primitive.BOOL : tp.Primitive < Primitive.BYTE || tp.Primitive == Primitive.CHAR)
                                throw new ParserException("O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            CompileInt32Conversion(assembler, tp);
                            break;

                        case Primitive.CHAR:
                            if (isExplicit ? tp.Primitive != Primitive.BOOL : tp.Primitive != Primitive.CHAR)
                                throw new ParserException("O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            CompileInt32Conversion(assembler, tp);
                            break;

                        case Primitive.SHORT:
                            if (isExplicit ? tp.Primitive == Primitive.BOOL : tp.Primitive < Primitive.SHORT)
                                throw new ParserException("O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            CompileInt32Conversion(assembler, tp);
                            break;

                        case Primitive.INT:
                            if (isExplicit ? tp.Primitive == Primitive.BOOL : tp.Primitive < Primitive.INT)
                                throw new ParserException("O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            CompileInt32Conversion(assembler, tp);
                            break;

                        case Primitive.LONG:
                            if (isExplicit ? tp.Primitive == Primitive.BOOL : tp.Primitive < Primitive.LONG)
                                throw new ParserException("O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

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
                                throw new ParserException("O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

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
                                throw new ParserException("O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

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
                        case Primitive.BOOL:
                        case Primitive.BYTE:
                        case Primitive.CHAR:
                        case Primitive.SHORT:
                            throw new ParserException("O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                        case Primitive.INT:
                            if (!isExplicit)
                                throw new ParserException("O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            break;

                        case Primitive.LONG:
                            if (!isExplicit)
                                throw new ParserException("O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            assembler.EmitInt64ToInt32();
                            break;

                        case Primitive.FLOAT:
                        case Primitive.DOUBLE:
                            throw new ParserException("O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");
                    }

                    return;
                }

                throw new ParserException("Unknow type '" + toType + "'.");
            }

            if (fromType is StructType s)
            {
                if (!s.Equals(toType))
                    throw new ParserException("O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                return;
            }

            if (fromType is ArrayType a)
            {
                if (!a.Equals(toType))
                    throw new ParserException("O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                return;
            }

            if (fromType is PointerType ptr)
            {
                if (toType is PrimitiveType tp)
                {
                    switch (tp.Primitive)
                    {
                        case Primitive.BOOL:
                            throw new ParserException("O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                        case Primitive.BYTE:
                            if (!isExplicit)
                                throw new ParserException("O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            break;

                        case Primitive.CHAR:
                            throw new ParserException("O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                        case Primitive.SHORT:
                            if (!isExplicit)
                                throw new ParserException("O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            break;

                        case Primitive.INT:
                            if (!isExplicit)
                                throw new ParserException("O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            break;

                        case Primitive.LONG:
                            if (!isExplicit)
                                throw new ParserException("O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            assembler.EmitInt32ToInt64();
                            break;

                        case Primitive.FLOAT:
                            if (!isExplicit)
                                throw new ParserException("O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            assembler.EmitInt32ToFloat32();
                            break;

                        case Primitive.DOUBLE:
                            if (!isExplicit)
                                throw new ParserException("O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            assembler.EmitInt32ToFloat64();
                            break;
                    }
                }

                if (toType is PointerType tptr)
                {
                    AbstractType otherType = tptr.Type;
                    if (isExplicit ? false : ptr.Type != otherType)
                        throw new ParserException("O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");
                }

                throw new ParserException("Unknow type '" + toType + "'.");
            }

            throw new ParserException("Unknow type '" + fromType + "'.");
        }

        private Expression ParsePrimaryExpression()
        {
            Token token = lexer.NextToken();
            if (token == null)
                throw new ParserException("End of expression reached but primary expression expected.");

            if (token is Keyword kw)
            {
                switch (kw.Value)
                {
                    case "verdadeiro":
                        return new BoolLiteralExpression(true);

                    case "falso":
                        return new BoolLiteralExpression(false);

                    case "nulo":
                        return new NullLiteralExpression();
                }
            }

            if (token is ByteLiteral b)
                return new ByteLiteralExpression(b.Value);

            if (token is CharLiteral c)
                return new CharLiteralExpression(c.Value);

            if (token is ShortLiteral s)
                return new ShortLiteralExpression(s.Value);

            if (token is IntLiteral i)
                return new IntLiteralExpression(i.Value);

            if (token is LongLiteral l)
                return new LongLiteralExpression(l.Value);

            if (token is FloatLiteral f)
                return new FloatLiteralExpression(f.Value);

            if (token is DoubleLiteral d)
                return new DoubleLiteralExpression(d.Value);

            if (token is StringLiteral str)
                return new StringLiteralExpression(str.Value);

            if (token is Identifier id)
            {
                if (lexer.NextSymbol("(", false) != null)
                {
                    CallExpression result = new CallExpression(new IdentifierExpression(id.Name));

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

                return new IdentifierExpression(id.Name);
            }

            if (token is Symbol symbol)
            {
                if (symbol.Value != "(")
                    throw new ParserException("'(' expected but '" + symbol.Value + "' found");

                Expression result = ParseExpression();

                lexer.NextSymbol(")");

                return result;
            }

            throw new ParserException("Unexpected token: " + token);
        }

        private Expression ParseUnaryExpression()
        {
            Symbol symbol = lexer.NextSymbol(false);
            if (symbol == null)
                return ParsePrimaryExpression();

            Expression operand;
            switch (symbol.Value)
            {
                case "+":
                    return ParsePrimaryExpression();

                case "-":
                    operand = ParsePrimaryExpression();
                    return new UnaryExpression(UnaryOperation.NEGATION, operand);

                case "*":
                    operand = ParsePrimaryExpression();
                    return new UnaryExpression(UnaryOperation.POINTER_DEFERENCE, operand);

                case "!":
                    operand = ParsePrimaryExpression();
                    return new UnaryExpression(UnaryOperation.LOGICAL_NOT, operand);

                case "~":
                    operand = ParsePrimaryExpression();
                    return new UnaryExpression(UnaryOperation.BITWISE_NOT, operand);
            }

            lexer.PreviusToken();
            return ParsePrimaryExpression();
        }
        private Expression ParseCastExpression()
        {
            if (lexer.NextKeyword("cast", false) == null)
                return ParseUnaryExpression();

            lexer.NextSymbol("<");
            AbstractType type = ParseType();
            lexer.NextSymbol(">");
            lexer.NextSymbol("(");
            Expression operand = ParseUnaryExpression();
            lexer.NextSymbol(")");

            return new CastExpression(type, operand);
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
                        leftOperand = new BinaryExpression(BinaryOperation.MUL, leftOperand, rightOperand);
                        break;

                    case "/":
                        rightOperand = ParseCastExpression();
                        leftOperand = new BinaryExpression(BinaryOperation.DIV, leftOperand, rightOperand);
                        break;

                    case "%":
                        rightOperand = ParseCastExpression();
                        leftOperand = new BinaryExpression(BinaryOperation.MOD, leftOperand, rightOperand);
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
                        leftOperand = new BinaryExpression(BinaryOperation.ADD, leftOperand, rightOperand);
                        break;

                    case "-":
                        rightOperand = ParseMultiplicativeExpression();
                        leftOperand = new BinaryExpression(BinaryOperation.SUB, leftOperand, rightOperand);
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
                    return new BinaryExpression(BinaryOperation.SHIFT_LEFT, leftOperand, rightOperand);

                case ">>":
                    rightOperand = ParseAdditiveExpression();
                    return new BinaryExpression(BinaryOperation.SHIFT_RIGHT, leftOperand, rightOperand);

                case ">>>":
                    rightOperand = ParseAdditiveExpression();
                    return new BinaryExpression(BinaryOperation.UNSIGNED_SHIFT_RIGHT, leftOperand, rightOperand);
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
                    return new BinaryExpression(BinaryOperation.GREATER, leftOperand, rightOperand);

                case ">=":
                    rightOperand = ParseShiftExpression();
                    return new BinaryExpression(BinaryOperation.GREATER_OR_EQUALS, leftOperand, rightOperand);

                case "<":
                    rightOperand = ParseShiftExpression();
                    return new BinaryExpression(BinaryOperation.LESS, leftOperand, rightOperand);

                case "<=":
                    rightOperand = ParseShiftExpression();
                    return new BinaryExpression(BinaryOperation.LESS_OR_EQUALS, leftOperand, rightOperand);
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
                    return new BinaryExpression(BinaryOperation.EQUALS, leftOperand, rightOperand);

                case "!=":
                    rightOperand = ParseInequalityExpression();
                    return new BinaryExpression(BinaryOperation.NOT_EQUALS, leftOperand, rightOperand);
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
                leftOperand = new BinaryExpression(BinaryOperation.BITWISE_AND, leftOperand, rightOperand);
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
                leftOperand = new BinaryExpression(BinaryOperation.BITWISE_AND, leftOperand, rightOperand);
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
                leftOperand = new BinaryExpression(BinaryOperation.BITWISE_OR, leftOperand, rightOperand);
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
                leftOperand = new BinaryExpression(BinaryOperation.LOGICAL_AND, leftOperand, rightOperand);
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
                leftOperand = new BinaryExpression(BinaryOperation.LOGICAL_XOR, leftOperand, rightOperand);
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
                leftOperand = new BinaryExpression(BinaryOperation.LOGICAL_OR, leftOperand, rightOperand);
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
                    return new BinaryExpression(BinaryOperation.STORE, leftOperand, rightOperand);

                case "+=":
                    rightOperand = ParseExpression();
                    return new BinaryExpression(BinaryOperation.STORE_ADD, leftOperand, rightOperand);

                case "-=":
                    rightOperand = ParseExpression();
                    return new BinaryExpression(BinaryOperation.STORE_SUB, leftOperand, rightOperand);

                case "*=":
                    rightOperand = ParseExpression();
                    return new BinaryExpression(BinaryOperation.STORE_MUL, leftOperand, rightOperand);

                case "/=":
                    rightOperand = ParseExpression();
                    return new BinaryExpression(BinaryOperation.STORE_DIV, leftOperand, rightOperand);

                case "%=":
                    rightOperand = ParseExpression();
                    return new BinaryExpression(BinaryOperation.STORE_MOD, leftOperand, rightOperand);

                case "&=":
                    rightOperand = ParseExpression();
                    return new BinaryExpression(BinaryOperation.STORE_AND, leftOperand, rightOperand);
                case "|=":
                    rightOperand = ParseExpression();
                    return new BinaryExpression(BinaryOperation.STORE_OR, leftOperand, rightOperand);

                case "^=":
                    rightOperand = ParseExpression();
                    return new BinaryExpression(BinaryOperation.STORE_XOR, leftOperand, rightOperand);

                case "<<=":
                    rightOperand = ParseExpression();
                    return new BinaryExpression(BinaryOperation.STORE_SHIFT_LEFT, leftOperand, rightOperand);

                case ">>=":
                    rightOperand = ParseExpression();
                    return new BinaryExpression(BinaryOperation.STORE_SHIFT_RIGHT, leftOperand, rightOperand);

                case ">>>=":
                    rightOperand = ParseExpression();
                    return new BinaryExpression(BinaryOperation.STORE_UNSIGNED_SHIFT_RIGHT, leftOperand, rightOperand);
            }

            lexer.PreviusToken();
            return leftOperand;
        }

        public AbstractType ParseType()
        {
            Keyword kw = lexer.NextKeyword(false);
            if (kw != null)
            {
                switch (kw.Value)
                {
                    case "bool":
                        return PrimitiveType.BOOL;

                    case "byte":
                        return PrimitiveType.BYTE;

                    case "char":
                        return PrimitiveType.CHAR;

                    case "short":
                        return PrimitiveType.SHORT;

                    case "int":
                        return PrimitiveType.INT;

                    case "long":
                        return PrimitiveType.LONG;

                    case "float":
                        return PrimitiveType.FLOAT;

                    case "real":
                        return PrimitiveType.DOUBLE;
                }

                lexer.PreviusToken();
            }

            Identifier id = lexer.NextIdentifier();
            StructType st = FindStruct(id.Name);
            if (st == null)
                throw new ParserException("Undeclared type '" + id.Name + "'");

            return st;
        }

        public void ParseParamsDeclaration(Function function)
        {
            while (true)
            {
                Identifier id = lexer.NextIdentifier();
                lexer.NextSymbol(":");
                AbstractType type = ParseType();

                Parameter p = function.DeclareParameter(id.Name, type);
                if (p == null)
                    throw new ParserException("Parameter '" + id.Name + "' already declared.");

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
                    throw new ParserException("Field '" + id.Name + "' already declared.");

                lexer.NextSymbol(";");

                if (lexer.NextSymbol("}", false) != null)
                    return;
            }
        }

        private void CompilePop(Assembler assembler, AbstractType type)
        {
            if (type != null)
            {
                if (type is PrimitiveType)
                {
                    PrimitiveType p = (PrimitiveType) type;
                    switch (p.Primitive)
                    {
                        case Primitive.BOOL:
                        case Primitive.BYTE:
                        case Primitive.CHAR:
                        case Primitive.SHORT:
                        case Primitive.INT:
                        case Primitive.FLOAT:
                            assembler.EmitPop();
                            break;

                        case Primitive.LONG:
                        case Primitive.DOUBLE:
                            assembler.EmitPop2();
                            break;
                    }
                }
                else
                    // TODO Implementar
                    throw new ParserException("???");
            }
        }

        private Statement ParseStatement()
        {
            if (lexer.NextSymbol(";", false) != null)
                return new EmptyStatement();

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
                    case "declare":
                    {
                        DeclarationStatement result = ParseVariableDeclaration();
                        lexer.NextSymbol(";");
                        return result;
                    }

                    case "se":
                    {
                        lexer.NextSymbol("(");

                        Expression expression = ParseExpression();
                        
                        lexer.NextSymbol(")");
            
                        Statement thenStatement = ParseStatement();

                        Statement elseStatement = null;
                        if (lexer.NextKeyword("senão", false) != null)
                            elseStatement = ParseStatement();

                        IfStatement result = new IfStatement(expression, thenStatement, elseStatement);
                        return result;
                    }

                    case "para":
                    {
                        lexer.NextSymbol("(");

                        ForStatement result = new ForStatement();

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

                        lexer.NextSymbol(")");

                        Statement statement = ParseStatement();

                        return new WhileStatement(expression, statement);
                    }

                    case "repita":
                    {
                        Statement statement = ParseStatement();

                        lexer.NextKeyword("enquanto");
                        lexer.NextSymbol("(");

                        Expression expression = ParseExpression();

                        lexer.NextSymbol(")");

                        return new DoStatement(expression, statement);
                    }

                    case "leia":
                    {
                        ReadStatement result = new ReadStatement();

                        Identifier id = lexer.NextIdentifier();
                        result.AddExpression(new IdentifierExpression(id.Name));

                        while (lexer.NextSymbol(",", false) != null)
                        {
                            id = lexer.NextIdentifier();
                            result.AddExpression(new IdentifierExpression(id.Name));
                        }

                        lexer.NextSymbol(";");

                        return result;
                    }

                    case "escreva":
                    {
                        PrintStatement result = new PrintStatement();

                        if (lexer.NextSymbol(";", false) != null)
                            throw new ParserException("Expressão esperada.");

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

                        if (lexer.NextSymbol(";", false) == null)
                        {
                            expression = ParseExpression();
                            lexer.NextSymbol(";");
                        }

                        return new ReturnStatement(expression);
                    }

                    case "quebra":
                    {
                        lexer.NextSymbol(";");

                        return new BreakStatement();
                    }
                }

                lexer.PreviusToken();
            }

            Expression expr = ParseExpression();
            lexer.NextSymbol(";");
            return new ExpressionStatement(expr);
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
                        throw new ParserException("Tipo de indexador inválido: '" + pt + "'.");
                }
            }
        }

        private AbstractType CompileAssignableExpression(Function function, Context context, Assembler assembler, Expression expression)
        {
            if (expression is UnaryExpression u)
            {
                Expression operand = u.Operand;
                AbstractType operandType = CompileExpression(function, context, assembler, operand);
                if (u.Operation != UnaryOperation.POINTER_DEFERENCE)
                    throw new ParserException("A expressão do lado esquerdo não é atribuível.");

                if (!(operandType is PointerType ptr))
                    throw new ParserException("Deferência de ponteiros só pode ser feita com um tipo 'pointer'.");

                return ptr.Type;
            }

            if (expression is FieldAcessorExpression f)
            {
                Expression operand = f.Operand;
                string fieldName = f.Field;

                AbstractType operandType = CompileAssignableExpression(function, context, assembler, operand);

                if (operandType is StructType s)
                {
                    Field field = s.FindField(fieldName);
                    if (field == null)
                        throw new ParserException("Campo '" + fieldName + "' não encontrado na struct: '" + s.Name + "'.");

                    assembler.EmitLoadConst(field.Offset);
                    assembler.EmitAdd();

                    return field.Type;
                }
                
                throw new ParserException("Acesso de membros em um tipo que não é struct: '" + operandType + "'.");
            }

            if (expression is ArrayAccessorExpression a)
            {
                Expression operand = a.Operand;               
                AbstractType operandType = CompileAssignableExpression(function, context, assembler, operand);

                if (operandType is ArrayType at)
                {
                    int rank = at.Rank;

                    if (a.IndexerCount == 0)
                        throw new ParserException("Não foi fornecido nenhum índice para o array.");

                    if (a.IndexerCount != rank)
                        throw new ParserException("Número de inídices fornecidos é diferente da dimensão do array.");

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

                    assembler.EmitAdd();

                    return at.Type;
                }

                throw new ParserException("Tipo '" + operandType + "' não é um array.");
            }

            if (expression is PrimaryExpression p)
            {
                if (p.PrimaryType != PrimaryType.IDENTIFIER)
                    throw new ParserException("Tipo de expressão não atribuível.");

                IdentifierExpression id = (IdentifierExpression) p;
                string name = id.Name;

                Variable var = context.FindVariable(id.Name);
                if (var == null)
                {
                    var = FindGlobalVariable(id.Name);
                    if (var == null)
                        throw new ParserException("Identificador'" + id.Name + "' não declarado.");

                    // variável local ou parâmetro
                    int offset = var.Offset;
                    assembler.EmitLoadConst(offset);
                }
                else
                {
                    // variável local ou parâmetro
                    int offset = var.Offset;
                    assembler.EmitLoadBP();
                    assembler.EmitLoadConst(offset);
                    assembler.EmitAdd();
                }

                return var.Type;
            }

            throw new ParserException("Tipo de expressão não atribuível.");
        }

        private void CompileLoad(Assembler assembler, AbstractType type)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BOOL:
                    case Primitive.BYTE:
                    case Primitive.CHAR:
                    case Primitive.SHORT:
                    case Primitive.INT:
                    case Primitive.FLOAT:
                        assembler.EmitLoadStack();
                        break;

                    case Primitive.LONG:
                    case Primitive.DOUBLE:
                        assembler.EmitLoadStack64();
                        break;
                }

                return;
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

            if (type is PointerType ptr)
            {
                assembler.EmitStoreStack();
                return;
            }

            throw new ParserException("Tipo desconhecido: '" + type + "'.");
        }

        private void CompileStore(Assembler assembler, AbstractType type)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BOOL:
                    case Primitive.BYTE:
                    case Primitive.CHAR:
                    case Primitive.SHORT:
                    case Primitive.INT:
                    case Primitive.FLOAT:
                        assembler.EmitStoreStack();
                        break;

                    case Primitive.LONG:
                    case Primitive.DOUBLE:
                        assembler.EmitStoreStack64();
                        break;
                }

                return;
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

            if (type is PointerType ptr)
            {
                assembler.EmitStoreStack();
                return;
            }

            throw new ParserException("Tipo desconhecido: '" + type + "'.");
        }

        private void CompileStoreAdd(Assembler assembler, AbstractType type)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                    case Primitive.SHORT:
                    case Primitive.INT:
                        assembler.EmitAdd();
                        assembler.EmitStoreStack();
                        return;

                    case Primitive.LONG:
                        assembler.EmitAdd64();
                        assembler.EmitStoreStack64();
                        return;

                    case Primitive.FLOAT:
                        assembler.EmitFAdd();
                        assembler.EmitStoreStack();
                        return;

                    case Primitive.DOUBLE:
                        assembler.EmitFAdd64();
                        assembler.EmitStoreStack64();
                        return;
                }
            }

            throw new ParserException("Operação inválida para o tipo '" + type + "'.");
        }

        private void CompileStoreSub(Assembler assembler, AbstractType type)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                    case Primitive.SHORT:
                    case Primitive.INT:
                        assembler.EmitSub();
                        assembler.EmitStoreStack();
                        return;

                    case Primitive.LONG:
                        assembler.EmitSub64();
                        assembler.EmitStoreStack64();
                        return;

                    case Primitive.FLOAT:
                        assembler.EmitFSub();
                        assembler.EmitStoreStack();
                        return;

                    case Primitive.DOUBLE:
                        assembler.EmitFSub64();
                        assembler.EmitStoreStack64();
                        return;
                }
            }

            throw new ParserException("Operação inválida para o tipo '" + type + "'.");
        }

        private void CompileStoreMul(Assembler assembler, AbstractType type)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                    case Primitive.SHORT:
                    case Primitive.INT:
                        assembler.EmitMul();
                        assembler.EmitStoreStack();
                        return;

                    case Primitive.LONG:
                        assembler.EmitMul64();
                        assembler.EmitStoreStack64();
                        return;

                    case Primitive.FLOAT:
                        assembler.EmitFMul();
                        assembler.EmitStoreStack();
                        return;

                    case Primitive.DOUBLE:
                        assembler.EmitFMul64();
                        assembler.EmitStoreStack64();
                        return;
                }
            }

            throw new ParserException("Operação inválida para o tipo '" + type + "'.");
        }

        private void CompileStoreDiv(Assembler assembler, AbstractType type)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                    case Primitive.SHORT:
                    case Primitive.INT:
                        assembler.EmitDiv();
                        assembler.EmitStoreStack();
                        return;

                    case Primitive.LONG:
                        assembler.EmitDiv64();
                        assembler.EmitStoreStack64();
                        return;

                    case Primitive.FLOAT:
                        assembler.EmitFDiv();
                        assembler.EmitStoreStack();
                        return;

                    case Primitive.DOUBLE:
                        assembler.EmitFDiv64();
                        assembler.EmitStoreStack64();
                        return;
                }
            }

            throw new ParserException("Operação inválida para o tipo '" + type + "'.");
        }

        private void CompileStoreMod(Assembler assembler, AbstractType type)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                    case Primitive.SHORT:
                    case Primitive.INT:
                        assembler.EmitMod();
                        assembler.EmitStoreStack();
                        return;

                    case Primitive.LONG:
                        assembler.EmitMod64();
                        assembler.EmitStoreStack64();
                        return;
                }
            }

            throw new ParserException("Operação inválida para o tipo '" + type + "'.");
        }

        private void CompileStoreAnd(Assembler assembler, AbstractType type)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BOOL:
                    case Primitive.BYTE:
                    case Primitive.SHORT:
                    case Primitive.INT:
                        assembler.EmitAnd();
                        assembler.EmitStoreStack();
                        return;

                    case Primitive.LONG:
                        assembler.EmitAnd64();
                        assembler.EmitStoreStack64();
                        return;
                }
            }

            throw new ParserException("Operação inválida para o tipo '" + type + "'.");
        }

        private void CompileStoreOr(Assembler assembler, AbstractType type)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BOOL:
                    case Primitive.BYTE:
                    case Primitive.SHORT:
                    case Primitive.INT:
                        assembler.EmitOr();
                        assembler.EmitStoreStack();
                        return;

                    case Primitive.LONG:
                        assembler.EmitOr64();
                        assembler.EmitStoreStack64();
                        return;
                }
            }

            throw new ParserException("Operação inválida para o tipo '" + type + "'.");
        }

        private void CompileStoreXor(Assembler assembler, AbstractType type)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BOOL:
                    case Primitive.BYTE:
                    case Primitive.SHORT:
                    case Primitive.INT:
                        assembler.EmitXor();
                        assembler.EmitStoreStack();
                        return;

                    case Primitive.LONG:
                        assembler.EmitXor64();
                        assembler.EmitStoreStack64();
                        return;
                }
            }

            throw new ParserException("Operação inválida para o tipo '" + type + "'.");
        }

        private void CompileStoreShiftLeft(Assembler assembler, AbstractType type)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                    case Primitive.SHORT:
                    case Primitive.INT:
                        assembler.EmitShl();
                        assembler.EmitStoreStack();
                        return;

                    case Primitive.LONG:
                        assembler.EmitShl64();
                        assembler.EmitStoreStack64();
                        return;
                }
            }

            throw new ParserException("Operação inválida para o tipo '" + type + "'.");
        }

        private void CompileStoreShiftRight(Assembler assembler, AbstractType type)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                    case Primitive.SHORT:
                    case Primitive.INT:
                        assembler.EmitShr();
                        assembler.EmitStoreStack();
                        return;

                    case Primitive.LONG:
                        assembler.EmitShr64();
                        assembler.EmitStoreStack64();
                        return;
                }
            }

            throw new ParserException("Operação inválida para o tipo '" + type + "'.");
        }

        private void CompileStoreUnsignedShiftRight(Assembler assembler, AbstractType type)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                    case Primitive.SHORT:
                    case Primitive.INT:
                        assembler.EmitUShr();
                        assembler.EmitStoreStack();
                        return;

                    case Primitive.LONG:
                        assembler.EmitUShr64();
                        assembler.EmitStoreStack64();
                        return;
                }
            }

            throw new ParserException("Operação inválida para o tipo '" + type + "'.");
        }

        private void CompileStoreExpression(Function function, Context context, Assembler assembler, BinaryOperation operation, Expression leftOperand, Expression rightOperand)
        {
            AbstractType leftType;
            AbstractType rightType;

            if (operation == BinaryOperation.STORE)
            {
                leftType = CompileAssignableExpression(function, context, assembler, leftOperand);
                rightType = CompileExpression(function, context, assembler, rightOperand);
                CompileCast(assembler, rightType, leftType, false);
                CompileStore(assembler, leftType);
                return;
            }

            Assembler tempAssembler = new Assembler();
            leftType = CompileAssignableExpression(function, context, tempAssembler, leftOperand);
            Assembler tempAssembler2 = new Assembler();
            tempAssembler2.Emit(tempAssembler);
            CompileLoad(tempAssembler2, leftType);
            assembler.Emit(tempAssembler);
            assembler.Emit(tempAssembler2);

            rightType = CompileExpression(function, context, assembler, rightOperand);
            CompileCast(assembler, rightType, leftType, false);

            switch (operation)
            {
                case BinaryOperation.STORE_OR:
                {
                    CompileStoreOr(assembler, leftType);
                    break;
                }

                case BinaryOperation.STORE_XOR:
                {
                    CompileStoreXor(assembler, leftType);
                    break;
                }

                case BinaryOperation.STORE_AND:
                {
                    CompileStoreAnd(assembler, leftType);
                    break;
                }

                case BinaryOperation.STORE_SHIFT_LEFT:
                {
                    CompileStoreShiftLeft(assembler, leftType);
                    break;
                }

                case BinaryOperation.STORE_SHIFT_RIGHT:
                {
                    CompileStoreShiftRight(assembler, leftType);
                    break;
                }

                case BinaryOperation.STORE_UNSIGNED_SHIFT_RIGHT:
                {
                    CompileStoreUnsignedShiftRight(assembler, leftType);
                    break;
                }

                case BinaryOperation.STORE_ADD:
                {
                    CompileStoreAdd(assembler, leftType);
                    break;
                }

                case BinaryOperation.STORE_SUB:
                {
                    CompileStoreSub(assembler, leftType);
                    break;
                }

                case BinaryOperation.STORE_MUL:
                {
                    CompileStoreMul(assembler, leftType);
                    break;
                }

                case BinaryOperation.STORE_DIV:
                {
                    CompileStoreDiv(assembler, leftType);
                    break;
                }

                case BinaryOperation.STORE_MOD:
                {
                    CompileStoreMod(assembler, leftType);
                    break;
                }

                default:
                    throw new ParserException("Operador '" + operation + "' desconhecido.");
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
                                    throw new ParserException("Operação não definida para o tipo '" + pt + "'.");

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
                        
                        throw new ParserException("Operação não definida para o tipo '" + operandType + "'.");
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
                                    throw new ParserException("Operação não definida para o tipo '" + pt + "'.");

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
                        
                        throw new ParserException("Operação não definida para o tipo '" + operandType + "'.");
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
                                    throw new ParserException("Operação não definida para o tipo '" + pt + "'.");
                            }

                            return PrimitiveType.BOOL;
                        }

                        throw new ParserException("Operação não definida para o tipo '" + operandType + "'.");
                    }

                    case UnaryOperation.POINTER_DEFERENCE:
                    {
                        AbstractType operandType = CompileExpression(function, context, assembler, operand);
                        if (operandType is PointerType ptr)
                        {
                            AbstractType ptrType = ptr.Type;
                            if (ptrType == null)
                                assembler.EmitLoadStack();
                            else if (ptrType is PrimitiveType pt)
                            {
                                switch (pt.Primitive)
                                {
                                    case Primitive.BOOL:
                                    case Primitive.BYTE:
                                    case Primitive.CHAR:
                                    case Primitive.SHORT:
                                    case Primitive.INT:
                                    case Primitive.FLOAT:
                                        assembler.EmitLoadStack();
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
                            else if (ptrType is PointerType ptrT)
                                assembler.EmitLoadStack();

                            return ptrType;
                        }
                        
                        throw new ParserException("Operação não definida para o tipo '" + operandType + "'.");
                    }

                    case UnaryOperation.PRE_INCREMENT: // ++x <=> x = x + 1
                    {
                        Assembler tempAssembler = new Assembler();
                        AbstractType operandType = CompileAssignableExpression(function, context, tempAssembler, operand);
                        assembler.Emit(tempAssembler);

                        assembler.Emit(tempAssembler);
                        CompileLoad(assembler, operandType);

                        if (operandType is PrimitiveType pt)
                        {
                            switch (pt.Primitive)
                            {
                                case Primitive.BOOL:
                                case Primitive.CHAR:
                                    throw new ParserException("Operação não definida para o tipo '" + pt + "'.");

                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitLoadConst(1);
                                    assembler.EmitAdd();
                                    assembler.EmitStoreStack();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitLoadConst(1L);
                                    assembler.EmitAdd64();
                                    assembler.EmitStoreStack64();
                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitLoadConst(1F);
                                    assembler.EmitFAdd();
                                    assembler.EmitStoreStack();
                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitLoadConst(1.0);
                                    assembler.EmitFAdd64();
                                    assembler.EmitStoreStack64();
                                    break;
                            }

                            assembler.Emit(tempAssembler);
                            CompileLoad(assembler, operandType);
                            return operandType;
                        }
                        
                        throw new ParserException("Operação não definida para o tipo '" + operandType + "'.");
                    }

                    case UnaryOperation.PRE_DECREMENT:
                    {
                        Assembler tempAssembler = new Assembler();
                        AbstractType operandType = CompileAssignableExpression(function, context, tempAssembler, operand);
                        assembler.Emit(tempAssembler);

                        assembler.Emit(tempAssembler);
                        CompileLoad(assembler, operandType);

                        if (operandType is PrimitiveType pt)
                        {
                            switch (pt.Primitive)
                            {
                                case Primitive.BOOL:
                                case Primitive.CHAR:
                                    throw new ParserException("Operação não definida para o tipo '" + pt + "'.");

                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitLoadConst(1);
                                    assembler.EmitSub();
                                    assembler.EmitStoreStack();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitLoadConst(1L);
                                    assembler.EmitSub64();
                                    assembler.EmitStoreStack64();
                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitLoadConst(1F);
                                    assembler.EmitFSub();
                                    assembler.EmitStoreStack();
                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitLoadConst(1.0);
                                    assembler.EmitFSub64();
                                    assembler.EmitStoreStack64();
                                    break;
                            }

                            assembler.Emit(tempAssembler);
                            CompileLoad(assembler, operandType);
                            return operandType;
                        }
                        
                        throw new ParserException("Operação não definida para o tipo '" + operandType + "'.");
                    }

                    case UnaryOperation.POST_INCREMENT:
                    {
                        Assembler tempAssembler = new Assembler();
                        AbstractType operandType = CompileAssignableExpression(function, context, tempAssembler, operand);
                        Assembler tempAssembler2 = new Assembler();
                        tempAssembler2.Emit(tempAssembler);
                        CompileLoad(tempAssembler2, operandType);
                        assembler.Emit(tempAssembler2);

                        assembler.Emit(tempAssembler);
                        assembler.Emit(tempAssembler2);

                        if (operandType is PrimitiveType pt)
                        {
                            switch (pt.Primitive)
                            {
                                case Primitive.BOOL:
                                case Primitive.CHAR:
                                    throw new ParserException("Operação não definida para o tipo '" + pt + "'.");

                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitLoadConst(1);
                                    assembler.EmitAdd();
                                    assembler.EmitStoreStack();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitLoadConst(1L);
                                    assembler.EmitAdd64();
                                    assembler.EmitStoreStack64();
                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitLoadConst(1F);
                                    assembler.EmitFAdd();
                                    assembler.EmitStoreStack();
                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitLoadConst(1.0);
                                    assembler.EmitFAdd64();
                                    assembler.EmitStoreStack64();
                                    break;
                            }

                            return operandType;
                        }
                        
                       throw new ParserException("Operação não definida para o tipo '" + operandType + "'.");
                    }

                    case UnaryOperation.POST_DECREMENT:
                    {
                        Assembler tempAssembler = new Assembler();
                        AbstractType operandType = CompileAssignableExpression(function, context, tempAssembler, operand);
                        Assembler tempAssembler2 = new Assembler();
                        tempAssembler2.Emit(tempAssembler);
                        CompileLoad(tempAssembler2, operandType);
                        assembler.Emit(tempAssembler2);

                        assembler.Emit(tempAssembler);
                        assembler.Emit(tempAssembler2);

                        if (operandType is PrimitiveType pt)
                        {
                            switch (pt.Primitive)
                            {
                                case Primitive.BOOL:
                                case Primitive.CHAR:
                                    throw new ParserException("Operação não definida para o tipo '" + pt + "'.");

                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitLoadConst(1);
                                    assembler.EmitSub();
                                    assembler.EmitStoreStack();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitLoadConst(1L);
                                    assembler.EmitSub64();
                                    assembler.EmitStoreStack64();
                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitLoadConst(1F);
                                    assembler.EmitFSub();
                                    assembler.EmitStoreStack();
                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitLoadConst(1.0);
                                    assembler.EmitFSub64();
                                    assembler.EmitStoreStack64();
                                    break;
                            }

                            return operandType;
                        }
                        
                        throw new ParserException("Operação não definida para o tipo '" + operandType + "'.");
                    }

                    default:
                        throw new ParserException("Operador '" + u.Operation + "' desconhecido.");
                }
            }

            if (expression is BinaryExpression b)
            {
                Expression leftOperand = b.LeftOperand;
                Expression rightOperand = b.RightOperand;

                if (b.Operation <= BinaryOperation.STORE_MOD)
                {
                    CompileStoreExpression(function, context, assembler, b.Operation, leftOperand, rightOperand);
                    return null;
                }

                Assembler leftAssembler = new Assembler();
                AbstractType leftType = CompileExpression(function, context, leftAssembler, leftOperand);

                Assembler rightAssembler = new Assembler();
                AbstractType rightType = CompileExpression(function, context, rightAssembler, rightOperand);

                switch (b.Operation)
                {
                    case BinaryOperation.LOGICAL_OR:
                    {
                        if (!PrimitiveType.IsPrimitiveBool(leftType) || !PrimitiveType.IsPrimitiveBool(rightType))
                            throw new ParserException("Operação não definida entre tipos não booleanos.");

                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);
                        assembler.EmitOr();
                        return PrimitiveType.BOOL;
                    }

                    case BinaryOperation.LOGICAL_XOR:
                    {
                        if (!PrimitiveType.IsPrimitiveBool(leftType) || !PrimitiveType.IsPrimitiveBool(rightType))
                            throw new ParserException("Operação não definida entre tipos não booleanos.");

                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);
                        assembler.EmitXor();
                        return PrimitiveType.BOOL;
                    }

                    case BinaryOperation.LOGICAL_AND:
                    {
                        if (!PrimitiveType.IsPrimitiveBool(leftType) || !PrimitiveType.IsPrimitiveBool(rightType))
                            throw new ParserException("Operação não definida entre tipos não booleanos.");

                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);
                        assembler.EmitAnd();
                        return PrimitiveType.BOOL;
                    }

                    case BinaryOperation.SHIFT_LEFT:
                    {
                        if (!PrimitiveType.IsUpTo32BitsInt(rightType))
                            throw new ParserException("Tipo inválido para o operando 2: '" + rightType + "'.");

                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);

                        if (PrimitiveType.IsUpTo32BitsInt(leftType))
                            assembler.EmitShl();
                        else if (PrimitiveType.Is64BitsInt(leftType))
                            assembler.EmitShl64();
                        else
                            throw new ParserException("Tipo inválido para o operando 1: '" + leftType + "'.");

                        return leftType;
                    }

                    case BinaryOperation.SHIFT_RIGHT:
                    {
                        if (!PrimitiveType.IsUpTo32BitsInt(rightType))
                            throw new ParserException("Tipo inválido para o operando 2: '" + rightType + "'.");

                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);

                        if (PrimitiveType.IsUpTo32BitsInt(leftType))
                            assembler.EmitShr();
                        else if (PrimitiveType.Is64BitsInt(leftType))
                            assembler.EmitShr64();
                        else
                            throw new ParserException("Tipo inválido para o operando 1: '" + leftType + "'.");

                        return leftType;
                    }

                    case BinaryOperation.UNSIGNED_SHIFT_RIGHT:
                    {
                        if (!PrimitiveType.IsUpTo32BitsInt(rightType))
                            throw new ParserException("Tipo inválido para o operando 2: '" + rightType + "'.");

                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);

                        if (PrimitiveType.IsUpTo32BitsInt(leftType))
                            assembler.EmitUShr();
                        else if (PrimitiveType.Is64BitsInt(leftType))
                            assembler.EmitUShr64();
                        else
                            throw new ParserException("Tipo inválido para o operando 1: '" + leftType + "'.");

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
                                CompileCast(leftAssembler, leftType, rightType, false);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false);
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

                        throw new ParserException("Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
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
                                CompileCast(leftAssembler, leftType, rightType, false);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false);
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

                        throw new ParserException("Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.GREATER:
                    {
                        if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false);
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

                        throw new ParserException("Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.GREATER_OR_EQUALS:
                    {
                        if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false);
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

                        throw new ParserException("Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.LESS:
                    {
                        if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false);
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

                        throw new ParserException("Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.LESS_OR_EQUALS:
                    {
                        if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false);
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

                        throw new ParserException("Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.BITWISE_OR:
                    {
                        if (PrimitiveType.IsPrimitiveInteger(leftType) && PrimitiveType.IsPrimitiveInteger(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false);
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

                            return resultType;
                        }

                        throw new ParserException("Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.BITWISE_XOR:
                    {
                        if (PrimitiveType.IsPrimitiveInteger(leftType) && PrimitiveType.IsPrimitiveInteger(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false);
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

                            return resultType;
                        }

                        throw new ParserException("Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.BITWISE_AND:
                    {
                        if (PrimitiveType.IsPrimitiveInteger(leftType) && PrimitiveType.IsPrimitiveInteger(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false);
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

                            return resultType;
                        }

                        throw new ParserException("Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.ADD:
                    {
                        if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false);
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

                            return resultType;
                        }

                        if (leftType is PointerType && PrimitiveType.IsPrimitiveInteger(rightType))
                        {
                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);
                            assembler.EmitAdd();
                            return leftType;
                        }

                        if (PrimitiveType.IsPrimitiveInteger(leftType) && rightType is PointerType)
                        {
                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);
                            assembler.EmitAdd();
                            return rightType;
                        }

                        throw new ParserException("Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.SUB:
                    {
                        if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false);
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

                            return resultType;
                        }

                        if (leftType is PointerType && PrimitiveType.IsPrimitiveInteger(rightType))
                        {
                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);
                            assembler.EmitSub();
                            return leftType;
                        }

                        throw new ParserException("Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.MUL:
                    {
                        if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false);
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
                            }

                            return resultType;
                        }

                        throw new ParserException("Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.DIV:
                    {
                        if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false);
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
                            }

                            return resultType;
                        }

                        throw new ParserException("Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.MOD:
                    {
                        if (PrimitiveType.IsPrimitiveInteger(leftType) && PrimitiveType.IsPrimitiveInteger(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false);
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
                            }

                            return resultType;
                        }

                        throw new ParserException("Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    default:
                        throw new ParserException("Operador '" + b.Operation + "' desconhecido.");
                }
            }

            if (expression is FieldAcessorExpression f)
            {
                Expression operand = f.Operand;
                string fieldName = f.Field;

                AbstractType operandType = CompileAssignableExpression(function, context, assembler, operand);

                if (operandType is StructType s)
                {
                    Field field = s.FindField(fieldName);
                    if (field == null)
                        throw new ParserException("Campo '" + fieldName + "' não encontrado na struct: '" + s.Name + "'.");

                    assembler.EmitLoadConst(field.Offset);
                    assembler.EmitAdd();
                    CompileLoad(assembler, field.Type);

                    return field.Type;
                }

                throw new ParserException("Acesso de membros em um tipo que não é struct: '" + operandType + "'.");
            }

            if (expression is ArrayAccessorExpression a)
            {
                Expression operand = a.Operand;
                AbstractType operandType = CompileAssignableExpression(function, context, assembler, operand);

                if (operandType is ArrayType at)
                {
                    int rank = at.Rank;

                    if (a.IndexerCount == 0)
                        throw new ParserException("Não foi fornecido nenhum índice para o array.");

                    if (a.IndexerCount != rank)
                        throw new ParserException("Número de inídices fornecidos é diferente da dimensão do array.");

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

                    assembler.EmitAdd();

                    CompileLoad(assembler, at.Type);

                    return at.Type;
                }

                throw new ParserException("Tipo '" + operandType + "' não é um array.");
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
                        // TODO Implementar
                        assembler.EmitLoadConst(0);
                        return PointerType.STRING;

                    case NullLiteralExpression l:
                        assembler.EmitLoadConst(0);
                        return PointerType.NULL;

                    case IdentifierExpression id:
                    {
                        string name = id.Name;

                        Variable var = context.FindVariable(id.Name);
                        if (var == null)
                        {
                            var = FindGlobalVariable(id.Name);
                            if (var == null)
                                throw new ParserException("Identificador'" + id.Name + "' não declarado.");

                            // variável local ou parâmetro
                            int offset = var.Offset;
                            assembler.EmitLoadConst(offset);
                        }
                        else
                        {
                            // variável local ou parâmetro
                            int offset = var.Offset;
                            assembler.EmitLoadBP();
                            assembler.EmitLoadConst(offset);
                            assembler.EmitAdd();
                        }

                        CompileLoad(assembler, var.Type);

                        return var.Type;
                    }
                }

                throw new ParserException("Unknow primary type '" + p + "'");
            }

            if (expression is CallExpression c)
            {
                Expression operand = c.Operand;
                if (operand is IdentifierExpression id)
                {
                    string functionName = id.Name;
                    Function func = FindFunction(id.Name);
                    if (func == null)
                        throw new ParserException("Função '" + id.Name + "' não declarada.");

                    AbstractType returnType = func.ReturnType;
                    if (returnType != null)
                    {
                        if (returnType is PrimitiveType pt)
                        {
                            switch (pt.Primitive)
                            {
                                case Primitive.BOOL:
                                case Primitive.BYTE:
                                case Primitive.CHAR:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitLoadConst(0);
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitLoadConst(0L);
                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitLoadConst(0F);
                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitLoadConst(0.0);
                                    break;
                            }
                        }
                        else
                            // TODO Implementar
                            throw new ParserException("???");
                    }

                    if (func.ParamCount != c.ParameterCount)
                        throw new ParserException("A quantidade de parâmetros fornecido é diferente da quantidade total de parâmetros esperada.");

                    for (int j = 0; j < func.ParamCount; j++)
                    {
                        Parameter parameter = func[j];
                        Expression expressionParameter = c[j];
                        AbstractType paramType = CompileExpression(function, context, assembler, expressionParameter);
                        CompileCast(assembler, paramType, parameter.Type, false);
                    }

                    assembler.EmitLabel(func.EntryLabel);
                    assembler.EmitCall();

                    int paramOffset = -func.ParameterOffset - 2;
                    if (paramOffset > 0)
                    {
                        assembler.EmitLoadSP();
                        assembler.EmitLoadConst(paramOffset);
                        assembler.EmitSub();
                        assembler.EmitStoreSP();
                    }

                    return func.ReturnType;
                }

                throw new ParserException("Expressão de nome de função inválida.");
            }

            if (expression is CastExpression cs)
            {
                Expression operand = cs.Operand;
                AbstractType type = cs.Type;

                AbstractType operandType = CompileExpression(function, context, assembler, operand);
                CompileCast(assembler, operandType, type, true);
                return type;
            }

            throw new ParserException("Tipo desconhecido de expressão: " + expression);
        }

        public Variable CheckVariable(Context context, string name)
        {
            Variable var = context.FindVariable(name);
            if (var == null)
            {
                var = FindGlobalVariable(name);
                if (var == null)
                    throw new ParserException("Variável '" + name + "' não declarada.");
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

                if (expr != null && function.ReturnType == null)
                    throw new ParserException("A função não possui tipo de retorno.");

                if (expr == null && function.ReturnType != null)
                    throw new ParserException("Expressão de retorno esperada.");

                if (expr != null)
                {
                    assembler.EmitLoadBP();
                    assembler.EmitLoadConst(function.ReturnOffset);
                    assembler.EmitAdd();
                    AbstractType returnType = CompileExpression(function, context, assembler, expr);
                    CompileCast(assembler, returnType, function.ReturnType, false);
                    CompileStore(assembler, function.ReturnType);
                }

                assembler.EmitLabel(function.ReturnLabel);
                assembler.EmitJump();
            }
            else if (statement is BreakStatement b)
            {
                Label breakLabel = context.FindNearestBreakLabel();
                if (breakLabel == null)
                    throw new ParserException("Instrução 'quebra' deve estar dentro de um loop.");

                lexer.NextSymbol(";");

                assembler.EmitLabel(breakLabel);
                assembler.EmitJump();
            }
            else if (statement is ReadStatement rd)
            {
                for (int j = 0; j < rd.ExpressionCount; j++)
                {
                    Expression expr = rd[j];
                    AbstractType exprType = CompileAssignableExpression(function, context, assembler, expr);

                    if (!(exprType is PrimitiveType p))
                        throw new ParserException("Expected an expression of primitive type.");

                    switch (p.Primitive)
                    {
                        case Primitive.BOOL:
                        case Primitive.BYTE:
                        case Primitive.CHAR:
                        case Primitive.SHORT:
                        case Primitive.INT:
                            assembler.EmitScan();
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
                    }
                }
            }
            else if (statement is PrintStatement p)
            {
                for (int j = 0; j < p.ExpressionCount; j++)
                {
                    Expression expr = p[j];

                    AbstractType exprType = CompileExpression(function, context, assembler, expr);

                    if (!(exprType is PrimitiveType pt))
                        throw new ParserException("Expected an expression of primitive type.");

                    switch (pt.Primitive)
                    {
                        case Primitive.BOOL:
                        case Primitive.BYTE:
                        case Primitive.CHAR:
                        case Primitive.SHORT:
                        case Primitive.INT:
                            assembler.EmitPrint();
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
                    }
                }
            }
            else if (statement is IfStatement i)
            {
                Expression expression = i.Expression;
                Statement thenStatement = i.ThenStatement;
                Statement elseStatement = i.ElseStatement;

                AbstractType exprType = CompileExpression(function, context, assembler, expression);
  
                if (!PrimitiveType.IsPrimitiveBool(exprType))
                    throw new ParserException("Expressão do tipo bool experada.");

                Label lblElse = CreateLabel();
                assembler.EmitLabel(lblElse);
                assembler.EmitJumpIfFalse();

                CompileStatement(function, context, assembler, thenStatement);

                Label lblEnd = CreateLabel();
                assembler.EmitLabel(lblEnd);
                assembler.EmitJump();

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

                assembler.EmitLabel(lblEnd);
                assembler.EmitJumpIfFalse();

                if (!PrimitiveType.IsPrimitiveBool(exprType))
                    throw new ParserException("Expressão do tipo bool experada.");

                CompileStatement(function, context, assembler, stm);

                assembler.EmitLabel(lblLoop);
                assembler.EmitJump();
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
                    throw new ParserException("Expressão do tipo bool experada.");

                assembler.EmitLabel(lblLoop);
                assembler.EmitJumpIfFalse();

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
                        throw new ParserException("Expressão do tipo bool esperada.");
                }
                else
                    assembler.EmitLoadConst(true);

                Label lblEnd = CreateLabel();
                context.PushBreakLabel(lblEnd);

                assembler.EmitLabel(lblEnd);
                assembler.EmitJumpIfFalse();

                Statement stm = f.Statement;
                CompileStatement(function, context, assembler, stm);

                // atualizadores
                for (int j = 0; j < f.UpdaterCount; j++)
                {
                    Expression updater = f.GetUpdater(j);
                    AbstractType updaterType = CompileExpression(function, context, assembler, updater);
                    CompilePop(assembler, updaterType);
                }

                assembler.EmitLabel(lblLoop);
                assembler.EmitJump();
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
                throw new ParserException("Tipo desconhecido de statement: " + statement);
        }

        public BlockStatement ParseBlock()
        {
            BlockStatement result = new BlockStatement();
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

            DeclarationStatement result = new DeclarationStatement(type);
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

                Variable var = function == null ? (Variable) DeclareGlobalVariable(name, type) : context.DeclareLocalVariable(function, name, type);
                if (var == null)
                    throw new ParserException("Variável '" + name + "' já declarada.");

                if (initializer != null)
                {
                    if (function == null)
                        throw new ParserException("Variável global não pode ser inicializada.");

                    if (var is GlobalVariable)
                        assembler.EmitLoadConst(var.Offset);
                    else
                    {
                        assembler.EmitLoadBP();
                        assembler.EmitLoadConst(var.Offset);
                        assembler.EmitAdd();
                    }

                    AbstractType initializerType = CompileExpression(function, context, assembler, initializer);
                    CompileCast(assembler, initializerType, type, false);
                    CompileStore(assembler, type);
                }
            }
        }

        public void ParseFunctionDeclaration(Assembler assembler)
        {
            Identifier id = lexer.NextIdentifier();
            Function f = DeclareFunction(id.Name);
            if (f == null)
                throw new ParserException("Function '" + id.Name + "' already declared.");

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
            Context context = new Context(f);
            Assembler tempAssembler = new Assembler();

            f.CreateReturnLabel();
            BlockStatement block = ParseBlock();
            CompileStatement(f, context, tempAssembler, block);
            f.BindReturnLabel(tempAssembler);

            f.CreateEntryLabel();
            f.BindEntryLabel(assembler);

            f.BeginBlock(assembler);
            assembler.Emit(tempAssembler);
            f.EndBlock(assembler);
        }

        public void ParseStructDeclaration()
        {
            Identifier id = lexer.NextIdentifier();
            StructType st = DeclareStruct(id.Name);
            if (st == null)
                throw new ParserException("Struct '" + id.Name + "' already declared.");

            lexer.NextSymbol("{");
            if (lexer.NextSymbol("}", false) == null)
                ParseFieldsDeclaration(st);
        }

        public bool ParseDeclaration(Assembler assembler)
        {
            Keyword kw = lexer.NextKeyword(false);
            if (kw != null)
            {
                switch (kw.Value)
                {
                    case "declare":
                    {
                        DeclarationStatement declaration = ParseVariableDeclaration();
                        CompileVariableDeclaration(null, null, null, declaration);
                        lexer.NextSymbol(";");
                        return true;
                    }

                    case "função":
                        ParseFunctionDeclaration(assembler);
                        return true;

                    case "estrutura":
                        ParseStructDeclaration();
                        return true;
                }

                lexer.PreviusToken();
            }

            lexer.NextSymbol("{");

            Function f = DeclareFunction("@main");
            if (f == null)
                throw new ParserException("Entry point already declared.");

            entryPoint = f;

            Context context = new Context(f);
            Assembler tempAssembler = new Assembler();

            f.CreateReturnLabel();
            BlockStatement block = ParseBlock();
            CompileStatement(f, context, tempAssembler, block);
            f.BindReturnLabel(tempAssembler);

            f.CreateEntryLabel();
            f.BindEntryLabel(assembler);

            f.BeginBlock(assembler);
            assembler.Emit(tempAssembler);
            f.EndBlock(assembler);

            return false;
        }

        public void ParseProgram(Assembler assembler)
        {
            lexer.NextKeyword("programa");
            lexer.NextIdentifier();
            lexer.NextSymbol("{");

            Assembler tempAssembler = new Assembler();
            while (ParseDeclaration(tempAssembler))
            {
            }

            lexer.NextSymbol("}");

            if (globalVariableOffset > 0)
            {
                assembler.EmitLoadSP();
                assembler.EmitLoadConst(globalVariableOffset);
                assembler.EmitAdd();
                assembler.EmitStoreSP();
            }

            if (entryPoint != null)
            {
                assembler.EmitLabel(entryPoint.EntryLabel);
                assembler.EmitCall();
                assembler.EmitHalt();
            }

            assembler.Emit(tempAssembler);
        }

        public AbstractType CompileAdditiveExpression(string expression, Assembler assembler)
        {
            globals.Clear();
            structs.Clear();
            functions.Clear();

            lexer.Input = expression;
            Context context = new Context(null);
            Expression expr = ParseAdditiveExpression();
            AbstractType type = CompileExpression(null, context, assembler, expr);

            Token token = lexer.NextToken();
            if (token != null)
                throw new ParserException("Fim da expressão esperado mas " + token + " encontrado.");

            return type;
        }

        public void CompileProgram(string source, Assembler assembler)
        {
            globals.Clear();
            structs.Clear();
            functions.Clear();

            lexer.Input = source;

            ParseProgram(assembler);

            for (int i = 0; i < labels.Count; i++)
            {
                Label label = labels[i];
                if (label.BindedIP != -1)
                    label.UpdateReferences();
            }

            Token token = lexer.NextToken();
            if (token != null)
                throw new ParserException("Fim do programa esperado mas " + token + " encontrado.");
        }
    }
}