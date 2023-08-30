using System.IO;

using Comp.Lex;
using Comp.Types;

namespace Comp;

public partial class Compiler
{
    private Expression ParsePrimaryExpression()
    {
        Token token = lexer.NextToken() ?? throw new CompilerException(lexer.CurrentInterval(), "Fim do arquivo encontrado mas token esperado.");

        switch (token)
        {
            case Keyword kw:
                switch (kw.Value)
                {
                    case "verdade":
                        return new BoolLiteralExpression(kw.Interval, true);

                    case "falso":
                        return new BoolLiteralExpression(kw.Interval, false);

                    case "nulo":
                        return new NullLiteralExpression(kw.Interval);
                }

                break;

            case ByteLiteral b:
                return new ByteLiteralExpression(token.Interval, b.Value);

            case CharLiteral c:
                return new CharLiteralExpression(token.Interval, c.Value);

            case ShortLiteral s:
                return new ShortLiteralExpression(token.Interval, s.Value);

            case IntLiteral i:
                return new IntLiteralExpression(token.Interval, i.Value);

            case LongLiteral l:
                return new LongLiteralExpression(token.Interval, l.Value);

            case FloatLiteral f:
                return new FloatLiteralExpression(token.Interval, f.Value);

            case DoubleLiteral d:
                return new DoubleLiteralExpression(token.Interval, d.Value);

            case StringLiteral str:
                return new StringLiteralExpression(token.Interval, str.Value);

            case Identifier id:
            {
                if (lexer.NextSymbol("(", false) != null)
                {
                    CallExpression result = new(id.Interval, new IdentifierExpression(id.Interval, id.Name));

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

            case Symbol symbol:
            {
                if (symbol.Value != "(")
                    throw new CompilerException(symbol.Interval, $"'(' esperado mas '{symbol.Value}' encontrado.");

                Expression result = ParseExpression();

                lexer.NextSymbol(")");

                return result;
            }
        }

        throw new CompilerException(token.Interval, $"Token não esperado: {token}");
    }

    private Expression ParsePostFixExpression()
    {
        Expression operand = ParsePrimaryExpression();

        while (true)
        {
            Symbol symbol = lexer.NextSymbol(false);
            if (symbol == null)
                return operand;

            switch (symbol.Value)
            {
                case "++":
                    operand = new UnaryExpression(operand.Interval.Merge(symbol.Interval), UnaryOperation.POST_INCREMENT, operand);
                    break;

                case "--":
                    operand = new UnaryExpression(operand.Interval.Merge(symbol.Interval), UnaryOperation.POST_DECREMENT, operand);
                    break;

                case ".":
                {
                    Identifier id = lexer.NextIdentifier();
                    operand = new FieldAccessorExpression(operand.Interval.Merge(id.Interval), operand, id.Name);
                    break;
                }

                case "[":
                {
                    if (lexer.NextSymbol("]", false) != null)
                        throw new CompilerException(operand.Interval, "Índice de array esperado.");

                    ArrayAccessorExpression result = new(operand.Interval.Merge(symbol.Interval), operand);
                    Expression indexer = ParseExpression();
                    result.AddIndexer(indexer);

                    while (lexer.NextSymbol(",", false) != null)
                    {
                        indexer = ParseExpression();
                        result.AddIndexer(indexer);
                    }

                    lexer.NextSymbol("]");
                    operand = result;
                    break;
                }

                default:
                    lexer.PreviusToken();
                    return operand;
            }
        }
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
                return ParseUnaryExpression();

            case "-":
                operand = ParseUnaryExpression();
                return new UnaryExpression(symbol.Interval.Merge(operand.Interval), UnaryOperation.NEGATION, operand);

            case "*":
                operand = ParseUnaryExpression();
                return new UnaryExpression(symbol.Interval.Merge(operand.Interval), UnaryOperation.POINTER_INDIRECTION, operand);

            case "!":
                operand = ParseUnaryExpression();
                return new UnaryExpression(symbol.Interval.Merge(operand.Interval), UnaryOperation.LOGICAL_NOT, operand);

            case "~":
                operand = ParseUnaryExpression();
                return new UnaryExpression(symbol.Interval.Merge(operand.Interval), UnaryOperation.BITWISE_NOT, operand);

            case "++":
                operand = ParseUnaryExpression();
                return new UnaryExpression(symbol.Interval.Merge(operand.Interval), UnaryOperation.PRE_INCREMENT, operand);

            case "--":
                operand = ParseUnaryExpression();
                return new UnaryExpression(symbol.Interval.Merge(operand.Interval), UnaryOperation.PRE_DECREMENT, operand);

            case "&":
                operand = ParseUnaryExpression();
                return new UnaryExpression(symbol.Interval.Merge(operand.Interval), UnaryOperation.POINTER_TO, operand);
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

        return new CastExpression(start.Interval.Merge(end.Interval), type, operand);
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
                    leftOperand = new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.MUL, leftOperand, rightOperand);
                    break;

                case "/":
                    rightOperand = ParseCastExpression();
                    leftOperand = new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.DIV, leftOperand, rightOperand);
                    break;

                case "%":
                    rightOperand = ParseCastExpression();
                    leftOperand = new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.MOD, leftOperand, rightOperand);
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
                    leftOperand = new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.ADD, leftOperand, rightOperand);
                    break;

                case "-":
                    rightOperand = ParseMultiplicativeExpression();
                    leftOperand = new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.SUB, leftOperand, rightOperand);
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
                return new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.SHIFT_LEFT, leftOperand, rightOperand);

            case ">>":
                rightOperand = ParseAdditiveExpression();
                return new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.SHIFT_RIGHT, leftOperand, rightOperand);

            case ">>>":
                rightOperand = ParseAdditiveExpression();
                return new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.UNSIGNED_SHIFT_RIGHT, leftOperand, rightOperand);
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
                return new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.GREATER, leftOperand, rightOperand);

            case ">=":
                rightOperand = ParseShiftExpression();
                return new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.GREATER_OR_EQUALS, leftOperand, rightOperand);

            case "<":
                rightOperand = ParseShiftExpression();
                return new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.LESS, leftOperand, rightOperand);

            case "<=":
                rightOperand = ParseShiftExpression();
                return new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.LESS_OR_EQUALS, leftOperand, rightOperand);
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
                return new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.EQUALS, leftOperand, rightOperand);

            case "!=":
                rightOperand = ParseInequalityExpression();
                return new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.NOT_EQUALS, leftOperand, rightOperand);
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
            leftOperand = new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.BITWISE_AND, leftOperand, rightOperand);
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
            leftOperand = new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.BITWISE_AND, leftOperand, rightOperand);
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
            leftOperand = new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.BITWISE_OR, leftOperand, rightOperand);
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
            leftOperand = new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.LOGICAL_AND, leftOperand, rightOperand);
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
            leftOperand = new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.LOGICAL_XOR, leftOperand, rightOperand);
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
            leftOperand = new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.LOGICAL_OR, leftOperand, rightOperand);
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
                return new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.STORE, leftOperand, rightOperand);

            case "+=":
                rightOperand = ParseExpression();
                return new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.STORE_ADD, leftOperand, rightOperand);

            case "-=":
                rightOperand = ParseExpression();
                return new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.STORE_SUB, leftOperand, rightOperand);

            case "*=":
                rightOperand = ParseExpression();
                return new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.STORE_MUL, leftOperand, rightOperand);

            case "/=":
                rightOperand = ParseExpression();
                return new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.STORE_DIV, leftOperand, rightOperand);

            case "%=":
                rightOperand = ParseExpression();
                return new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.STORE_MOD, leftOperand, rightOperand);

            case "&=":
                rightOperand = ParseExpression();
                return new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.STORE_AND, leftOperand, rightOperand);
            case "|=":
                rightOperand = ParseExpression();
                return new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.STORE_OR, leftOperand, rightOperand);

            case "^=":
                rightOperand = ParseExpression();
                return new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.STORE_XOR, leftOperand, rightOperand);

            case "<<=":
                rightOperand = ParseExpression();
                return new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.STORE_SHIFT_LEFT, leftOperand, rightOperand);

            case ">>=":
                rightOperand = ParseExpression();
                return new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.STORE_SHIFT_RIGHT, leftOperand, rightOperand);

            case ">>>=":
                rightOperand = ParseExpression();
                return new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.STORE_UNSIGNED_SHIFT_RIGHT, leftOperand, rightOperand);
        }

        lexer.PreviusToken();
        return leftOperand;
    }

    private AbstractType ParseType(bool allowVoid = false)
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

                case "texto":
                    result = StringType.STRING;
                    break;

                default:
                    lexer.PreviusToken();
                    break;
            }
        }

        if (result == null)
        {
            Identifier id = lexer.NextIdentifier();
            string name = id.Name;
            result = unity.FindStruct(name);
            if (result == null)
                result = unity.AddUndeclaredType(name, id.Interval);
        }

        while (true)
        {
            if (lexer.NextSymbol("[", false) != null)
            {
                if (PrimitiveType.IsPrimitiveVoid(result))
                    throw new CompilerException(kw.Interval, "Não é possível criar um array do tipo void.");

                if (lexer.NextSymbol("]", false) != null)
                {
                    result = new PointerType(result, true);
                }
                else
                {
                    ArrayType a = new(result);

                    NumericLiteral number = lexer.NextNumber();
                    if (number is not IntLiteral n)
                        throw new CompilerException(number.Interval, "Literal inteiro esperado.");

                    if (n.Value <= 0)
                        throw new CompilerException(number.Interval, "Tamanho de array deve ser maior que zero.");

                    a.AddBoundary(n.Value);

                    while (lexer.NextSymbol(",", false) != null)
                    {
                        number = lexer.NextNumber();
                        if (number is not IntLiteral n2)
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
                return !allowVoid && PrimitiveType.IsPrimitiveVoid(result)
                    ? throw new CompilerException(kw.Interval, "Tipo void não permitido nesta declaração.")
                    : result;
            }
        }
    }

    private void ParseParamsDeclaration(Function function)
    {
        while (true)
        {
            bool byRef = lexer.NextSymbol("&", false) != null;

            Identifier id = lexer.NextIdentifier();
            string name = id.Name;
            lexer.NextSymbol(":");
            AbstractType type = ParseType();

            _ = function.DeclareParameter(name, type, id.Interval, byRef) ?? throw new CompilerException(id.Interval, $"Parâmetro '{name}' já declarado.");

            if (lexer.NextSymbol(",", false) == null)
                break;
        }
    }

    private void ParseFieldsDeclaration(StructType st)
    {
        while (true)
        {
            Identifier id = lexer.NextIdentifier();
            string name = id.Name;
            lexer.NextSymbol(":");
            AbstractType type = ParseType();

            _ = st.DeclareField(name, type, id.Interval) ?? throw new CompilerException(id.Interval, $"Campo '{name}' já declarado.");

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
            return new EmptyStatement(lexer.CurrentInterval());

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

                    IfStatement result = new(kw.Interval.Merge(end.Interval), expression, thenStatement, elseStatement);
                    return result;
                }

                case "para":
                {
                    lexer.NextSymbol("(");

                    ForStatement result = new(kw.Interval);

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

                    return new WhileStatement(kw.Interval.Merge(end.Interval), expression, statement);
                }

                case "repita":
                {
                    Statement statement = ParseStatement();

                    lexer.NextKeyword("enquanto");
                    lexer.NextSymbol("(");

                    Expression expression = ParseExpression();

                    Symbol end = lexer.NextSymbol(")");

                    return new DoStatement(kw.Interval.Merge(end.Interval), expression, statement);
                }

                case "leia":
                {
                    ReadStatement result = new(kw.Interval);

                    if (lexer.NextSymbol(";", false) != null)
                        throw new CompilerException(lexer.CurrentInterval(), "Expressão esperada.");

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
                    PrintStatement result = new(kw.Interval, kw.Value == "escrevaln");

                    if (lexer.NextSymbol(";", false) != null)
                    {
                        return result.LineBreak ? (Statement) result : throw new CompilerException(lexer.CurrentInterval(), "Expressão esperada.");
                    }

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

                    return new ReturnStatement(kw.Interval.Merge(end.Interval), expression);
                }

                case "quebra":
                {
                    Symbol end = lexer.NextSymbol(";");

                    return new BreakStatement(kw.Interval.Merge(end.Interval));
                }
            }

            lexer.PreviusToken();
        }

        Expression expr = ParseExpression();
        lexer.NextSymbol(";");
        return new ExpressionStatement(expr.Interval, expr);
    }

    private BlockStatement ParseBlock()
    {
        BlockStatement result = new(lexer.CurrentInterval());

        while (lexer.NextSymbol("}", false) == null)
        {
            Statement statement = ParseStatement();
            result.AddStatement(statement);
        }

        result.Interval = result.Interval.Merge(lexer.CurrentInterval());
        return result;
    }

    private DeclarationStatement ParseVariableDeclaration(bool allowInitializer = true)
    {
        Identifier id = lexer.NextIdentifier();
        string name = id.Name;
        lexer.NextSymbol(":");
        AbstractType type = ParseType();

        Expression initializer = null;
        if (allowInitializer && lexer.NextSymbol("=", false) != null)
            initializer = ParseExpression();

        DeclarationStatement result = new(id.Interval, type);
        result.AddVariable(name, initializer);
        return result;
    }

    private void ParseFunctionDeclaration()
    {
        bool isExtern = lexer.NextKeyword("externa", false) != null;

        Identifier id = lexer.NextIdentifier();
        string name = id.Name;
        Function f = unity.DeclareFunction(name, id.Interval, isExtern) ?? throw new CompilerException(id.Interval, $"Função '{name}' já declarada.");

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

        if (isExtern)
        {
            lexer.NextSymbol(";");
        }
        else
        {
            lexer.NextSymbol("{");

            f.CreateEntryLabel();
            f.CreateReturnLabel();
            f.Block = ParseBlock();
        }

        f.Interval = f.Interval.Merge(lexer.CurrentInterval());
    }

    private void ParseStructDeclaration()
    {
        Identifier id = lexer.NextIdentifier();
        string name = id.Name;
        StructType st = unity.DeclareStruct(name, id.Interval) ?? throw new CompilerException(id.Interval, $"Tipo nomeado '{name}' já declarado.");

        lexer.NextSymbol("{");
        if (lexer.NextSymbol("}", false) == null)
            ParseFieldsDeclaration(st);
    }

    private void AddImport(SourceInterval interval, string unityName)
    {
        CompilationUnity.ImportResult r = unity.AddImport(unityName, out _);
        switch (r)
        {
            case CompilationUnity.ImportResult.SELF_REFERENCE_UNITY:
                throw new CompilerException(interval, "Não se pode usar uma unidade dentro dela própria.");

            case CompilationUnity.ImportResult.UNITY_ALREADY_IMPORTED:
                throw new CompilerException(interval, $"Unidade '{unityName}' já está sendo usada.");

            case CompilationUnity.ImportResult.UNITY_IS_PROGRAM:
                throw new CompilerException(interval, "Não se pode usar um programa dentro de uma unidade ou programa.");

            case CompilationUnity.ImportResult.UNITY_NOT_FOUND:
                throw new CompilerException(interval, $"Unidade '{unityName}' não encontrada.");
        }
    }

    private bool ParseDeclaration(bool endsWithBraces = true)
    {
        Keyword kw = lexer.NextKeyword(false);
        if (kw != null)
        {
            switch (kw.Value)
            {
                case "usando":
                {
                    Identifier id = lexer.NextIdentifier();
                    AddImport(id.Interval, id.Name);

                    while (lexer.NextSymbol(",", false) != null)
                    {
                        id = lexer.NextIdentifier();
                        AddImport(id.Interval, id.Name);
                    }

                    lexer.NextSymbol(";");

                    return true;
                }

                case "var":
                {
                    DeclarationStatement declaration = ParseVariableDeclaration(false);
                    var (name, _) = declaration[0];

                    _ = unity.DeclareGlobalVariable(name, declaration.Type, declaration.Interval) ?? throw new CompilerException(declaration.Interval, $"Variável global '{name}' já declarada.");

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

        Symbol start = lexer.NextSymbol("{", false);
        if (start != null)
        {
            Function f = unity.DeclareFunction("@main", start.Interval, false);
            unity.EntryPoint = f ?? throw new CompilerException(start.Interval, "Ponto de entrada já declarado.");

            f.CreateEntryLabel();
            f.CreateReturnLabel();
            f.Block = ParseBlock();
            f.Interval = f.Interval.Merge(lexer.CurrentInterval());

            return true;
        }

        if (endsWithBraces)
            lexer.NextSymbol("}", true, "Declaração ou '}' esperados.");

        return false;
    }

    private CompilationUnity ParseCompilationUnityFromFile(string fileName, bool programOnly = false)
    {
        return ParseCompilationUnity(fileName, File.OpenText(fileName), programOnly);
    }

#pragma warning disable IDE0051 // Remover membros privados não utilizados
    private CompilationUnity ParseCompilationUnityFromSource(int sourceID, string source, bool programOnly = false)
    {
        return ParseCompilationUnity("#" + sourceID, new StringReader(source), programOnly);
    }
#pragma warning restore IDE0051 // Remover membros privados não utilizados

    private CompilationUnity ParseCompilationUnity(string fileName, TextReader input, bool programOnly = false)
    {
        CompilationUnity oldUnity = unity;
        Lexer oldLexer = lexer;

        using (lexer = Lexer.CreateFromReader(fileName, input))
        {
            bool isUnity = false;
            if (lexer.NextKeyword("programa", false) == null)
            {
                if (programOnly)
                    throw new CompilerException(lexer.CurrentInterval(), "'programa' esperado.");

                if (lexer.NextKeyword("unidade", false) == null)
                    throw new CompilerException(lexer.CurrentInterval(), "'programa' ou 'unidade' esperado.");

                isUnity = true;
            }
            else if (program != null)
            {
                throw new CompilerException(lexer.CurrentInterval(), "Não pode haver mais que um programa.");
            }

            Identifier id = lexer.NextIdentifier();
            string name = id.Name;

            unity = new CompilationUnity(this, name, fileName, isUnity)
            {
                parsed = true
            };

            if (unity.Name != "System")
                unity.AddImport(unitySystem, out _);

            if (!isUnity)
                program = unity;

            bool endsWithBraces;
            if (!lexer.IsNextSymbol("{"))
            {
                lexer.NextSymbol(";", true, "Declaração, '{' ou ';' esperados.");
                endsWithBraces = false;
            }
            else
            {
                endsWithBraces = true;
            }

            while (ParseDeclaration(endsWithBraces))
            {
            }

            Token token = lexer.NextToken();
            if (token != null)
                throw new CompilerException(token.Interval, $"Fim do arquivo esperado mas {token} encontrado.");

            globalVariableOffset += unity.GlobalVariableSize;
        }

        CompilationUnity result = unity;
        lexer = oldLexer;
        unity = oldUnity;
        return result;
    }

#pragma warning disable IDE0051 // Remover membros privados não utilizados
    private void ParseCompilationUnity(CompilationUnity unity)
#pragma warning restore IDE0051 // Remover membros privados não utilizados
    {
        CompilationUnity oldUnity = this.unity;
        this.unity = unity;
        Lexer oldLexer = lexer;
        unity.parsed = true;

        using (lexer = Lexer.CreateFromFile(unity.FileName))
        {
            if (lexer.NextKeyword("unidade", false) == null)
                throw new CompilerException(lexer.CurrentInterval(), "'unidade' esperado.");

            Identifier id = lexer.NextIdentifier();
            string name = id.Name;

            if (unity.Name != name)
                throw new CompilerException(id.Interval, "Nome da unidade é diferente do nome do arquivo.");

            bool endsWithBraces;
            if (!lexer.IsNextSymbol("{"))
            {
                lexer.NextSymbol(";", true, "Declaração, '{' ou ';' esperados.");
                endsWithBraces = false;
            }
            else
            {
                endsWithBraces = true;
            }

            while (ParseDeclaration(endsWithBraces))
            {
            }

            Token token = lexer.NextToken();
            if (token != null)
                throw new CompilerException(token.Interval, $"Fim do arquivo esperado mas {token} encontrado.");

            globalVariableOffset += unity.GlobalVariableSize;
        }

        lexer = oldLexer;
        this.unity = oldUnity;
    }
}
