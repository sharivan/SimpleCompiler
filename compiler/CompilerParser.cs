using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using compiler.lexer;
using compiler.types;

namespace compiler
{
    public partial class Compiler
    {
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

        public AbstractType ParseType(bool allowVoid = false)
        {
            AbstractType result = null;

            if (lexer.NextSymbol("*", false) != null)
            {
                AbstractType type = ParseType(true);
                return new PointerType(type);
            }

            Keyword kw = lexer.NextKeyword(false);
            if (kw != null)
            {
                switch (kw.Value)
                {
                    case "void":
                        if (!allowVoid)
                            throw new CompilerException(kw.Interval, "Tipo void não permitido nesta declaração.");

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
                        throw new CompilerException(kw.Interval, "Não é possível criar um array do tipo void.");

                    if (lexer.NextSymbol("]", false) != null)
                        result = new PointerType(result, true);
                    else
                    {
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
                }
                else
                {
                    if (!allowVoid && PrimitiveType.IsPrimitiveVoid(result))
                        throw new CompilerException(kw.Interval, "Tipo void não permitido nesta declaração.");

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

        private InitializerStatement ParseInitializerStatement()
        {
            if (lexer.NextKeyword("var", false) != null)
            {
                DeclarationStatement result = ParseVariableDeclaration();
                return result;
            }

            Expression expr = ParseExpression();
            return new ExpressionStatement(expr.Interval, expr);
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
                                InitializerStatement initializer = ParseInitializerStatement();
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
    }
}
