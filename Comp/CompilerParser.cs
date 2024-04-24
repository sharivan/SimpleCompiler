using System.IO;

using Comp.Lex;
using Comp.Types;

namespace Comp;

public partial class Compiler
{
    private Expression ParsePrimaryExpression()
    {
        var token = lexer.NextToken() ?? throw new CompilerException(lexer.CurrentInterval(), "Fim do arquivo encontrado mas token esperado.");

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
                        var parameter = ParseExpression();
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

                var result = ParseExpression();

                lexer.NextSymbol(")");

                return result;
            }
        }

        throw new CompilerException(token.Interval, $"Token não esperado: {token}");
    }

    private Expression ParsePostFixExpression()
    {
        var operand = ParsePrimaryExpression();

        while (true)
        {
            var symbol = lexer.NextSymbol(false);
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
                    var id = lexer.NextIdentifier();
                    operand = new FieldAccessorExpression(operand.Interval.Merge(id.Interval), operand, id.Name);
                    break;
                }

                case "[":
                {
                    if (lexer.NextSymbol("]", false) != null)
                        throw new CompilerException(operand.Interval, "Índice de array esperado.");

                    ArrayAccessorExpression result = new(operand.Interval.Merge(symbol.Interval), operand);
                    var indexer = ParseExpression();
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
        var symbol = lexer.NextSymbol(false);
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

        var start = lexer.NextSymbol("<");
        var type = ParseType();
        lexer.NextSymbol(">");
        lexer.NextSymbol("(");
        var operand = ParseExpression();
        var end = lexer.NextSymbol(")");

        return new CastExpression(start.Interval.Merge(end.Interval), type, operand);
    }

    private Expression ParseMultiplicativeExpression()
    {
        var leftOperand = ParseCastExpression();
        while (true)
        {
            var symbol = lexer.NextSymbol(false);
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
        var leftOperand = ParseMultiplicativeExpression();
        while (true)
        {
            var symbol = lexer.NextSymbol(false);
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
        var leftOperand = ParseAdditiveExpression();

        var symbol = lexer.NextSymbol(false);
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
        var leftOperand = ParseShiftExpression();

        var symbol = lexer.NextSymbol(false);
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
        var leftOperand = ParseInequalityExpression();

        var symbol = lexer.NextSymbol(false);
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
        var leftOperand = ParseEqualityExpression();
        while (true)
        {
            if (lexer.NextSymbol("&", false) == null)
                return leftOperand;

            var rightOperand = ParseEqualityExpression();
            leftOperand = new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.BITWISE_AND, leftOperand, rightOperand);
        }
    }

    private Expression ParseBitwiseXorExpression()
    {
        var leftOperand = ParseBitwiseAndExpression();
        while (true)
        {
            if (lexer.NextSymbol("|", false) == null)
                return leftOperand;

            var rightOperand = ParseBitwiseAndExpression();
            leftOperand = new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.BITWISE_AND, leftOperand, rightOperand);
        }
    }

    private Expression ParseBitwiseOrExpression()
    {
        var leftOperand = ParseBitwiseXorExpression();
        while (true)
        {
            if (lexer.NextSymbol("|", false) == null)
                return leftOperand;

            var rightOperand = ParseBitwiseXorExpression();
            leftOperand = new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.BITWISE_OR, leftOperand, rightOperand);
        }
    }

    private Expression ParseLogicalAndExpression()
    {
        var leftOperand = ParseBitwiseOrExpression();
        while (true)
        {
            if (lexer.NextSymbol("&&", false) == null)
                return leftOperand;

            var rightOperand = ParseBitwiseOrExpression();
            leftOperand = new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.LOGICAL_AND, leftOperand, rightOperand);
        }
    }

    private Expression ParseLogicalXorExpression()
    {
        var leftOperand = ParseLogicalAndExpression();
        while (true)
        {
            if (lexer.NextSymbol("^^", false) == null)
                return leftOperand;

            var rightOperand = ParseLogicalAndExpression();
            leftOperand = new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.LOGICAL_XOR, leftOperand, rightOperand);
        }
    }

    private Expression ParseLogicalOrExpression()
    {
        var leftOperand = ParseLogicalXorExpression();
        while (true)
        {
            if (lexer.NextSymbol("||", false) == null)
                return leftOperand;

            var rightOperand = ParseLogicalXorExpression();
            leftOperand = new BinaryExpression(leftOperand.Interval.Merge(rightOperand.Interval), BinaryOperation.LOGICAL_OR, leftOperand, rightOperand);
        }
    }

    private Expression ParseExpression()
    {
        var leftOperand = ParseLogicalOrExpression();

        var symbol = lexer.NextSymbol(false);
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
            var type = ParseType(true);
            return new PointerType(type);
        }

        var kw = lexer.NextKeyword(false);
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
            var id = lexer.NextIdentifier();
            string name = id.Name;
            result = unity.FindFieldAggregation(name);
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

                    var number = lexer.NextNumber();
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

            var id = lexer.NextIdentifier();
            string name = id.Name;
            lexer.NextSymbol(":");
            var type = ParseType();

            _ = function.DeclareParameter(name, type, id.Interval, byRef) ?? throw new CompilerException(id.Interval, $"Parâmetro '{name}' já declarado.");

            if (lexer.NextSymbol(",", false) == null)
                break;
        }
    }

    private void ParseFieldsDeclaration(FieldAggregationType st)
    {
        while (true)
        {
            var id = lexer.NextIdentifier();
            string name = id.Name;
            lexer.NextSymbol(":");
            var type = ParseType();

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
            var result = ParseVariableDeclaration();
            return result;
        }

        var expr = ParseExpression();
        return new ExpressionStatement(expr.Interval, expr);
    }

    private Statement ParseStatement()
    {
        if (lexer.NextSymbol(";", false) != null)
            return new EmptyStatement(lexer.CurrentInterval());

        if (lexer.NextSymbol("{", false) != null)
        {
            var result = ParseBlock();
            return result;
        }

        Symbol end;
        SourceInterval interval;

        var kw = lexer.NextKeyword(false);
        if (kw != null)
        {
            interval = kw.Interval;
            switch (kw.Value)
            {
                case "var":
                {
                    var result = ParseVariableDeclaration();
                    lexer.NextSymbol(";");
                    return result;
                }

                case "se":
                {
                    lexer.NextSymbol("(");
                    var expression = ParseExpression();
                    lexer.NextSymbol(")");

                    var thenStatement = ParseStatement();
                    interval = interval.Merge(thenStatement.Interval);

                    Statement elseStatement = null;
                    if (lexer.NextKeyword("senão", false) != null)
                    {
                        elseStatement = ParseStatement();
                        interval = interval.Merge(elseStatement.Interval);
                    }

                    IfStatement result = new(interval, expression, thenStatement, elseStatement);
                    return result;
                }

                case "para":
                {
                    lexer.NextSymbol("(");

                    ForStatement result = new(interval);

                    // inicializadores
                    if (lexer.NextSymbol(";", false) == null)
                    {
                        do
                        {
                            var initializer = ParseInitializerStatement();
                            result.AddInitializer(initializer);
                        }
                        while (lexer.NextSymbol(",", false) != null);

                        lexer.NextSymbol(";");
                    }

                    // expressão de controle
                    if (lexer.NextSymbol(";", false) == null)
                    {
                        var expression = ParseExpression();
                        result.Expression = expression;

                        lexer.NextSymbol(";");
                    }

                    // atualizadores
                    if (lexer.NextSymbol(")", false) == null)
                    {
                        do
                        {
                            var updater = ParseExpression();
                            result.AddUpdater(updater);
                        }
                        while (lexer.NextSymbol(",", false) != null);

                        lexer.NextSymbol(")");
                    }

                    var statement = ParseStatement();
                    result.Statement = statement;

                    result.Interval = result.Interval.Merge(statement.Interval);
                    return result;
                }

                case "enquanto":
                {
                    lexer.NextSymbol("(");
                    var expression = ParseExpression();
                    end = lexer.NextSymbol(")");
                    var statement = ParseStatement();

                    return new WhileStatement(interval.Merge(end.Interval), expression, statement);
                }

                case "repita":
                {
                    var statement = ParseStatement();
                    lexer.NextKeyword("enquanto");
                    lexer.NextSymbol("(");
                    var expression = ParseExpression();
                    end = lexer.NextSymbol(")");

                    interval = interval.Merge(statement.Interval).Merge(end.Interval);
                    return new DoStatement(interval, expression, statement);
                }

                case "leia":
                {
                    ReadStatement result = new(interval);

                    if (lexer.NextSymbol(";", false) != null)
                        throw new CompilerException(lexer.CurrentInterval(), "Expressão esperada.");

                    var expression = ParseExpression();
                    result.AddExpression(expression);

                    while (lexer.NextSymbol(",", false) != null)
                    {
                        expression = ParseExpression();
                        result.AddExpression(expression);
                    }

                    end = lexer.NextSymbol(";");

                    result.Interval = result.Interval.Merge(end.Interval);
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

                    var expression = ParseExpression();
                    result.AddExpression(expression);

                    while (lexer.NextSymbol(",", false) != null)
                    {
                        expression = ParseExpression();
                        result.AddExpression(expression);
                    }

                    end = lexer.NextSymbol(";");

                    result.Interval = result.Interval.Merge(end.Interval);
                    return result;
                }

                case "retorne":
                {
                    Expression expression = null;

                    if ((end = lexer.NextSymbol(";", false)) == null)
                    {
                        expression = ParseExpression();
                        end = lexer.NextSymbol(";");
                    }

                    interval = interval.Merge(end.Interval);
                    return new ReturnStatement(interval, expression);
                }

                case "quebra":
                {
                    end = lexer.NextSymbol(";");

                    interval = interval.Merge(end.Interval);
                    return new BreakStatement(interval);
                }
            }

            lexer.PreviusToken();
        }

        var expr = ParseExpression();
        end = lexer.NextSymbol(";");

        interval = expr.Interval.Merge(end.Interval);
        return new ExpressionStatement(interval, expr);
    }

    private BlockStatement ParseBlock()
    {
        BlockStatement result = new(lexer.CurrentInterval());

        while (lexer.NextSymbol("}", false) == null)
        {
            var statement = ParseStatement();
            result.AddStatement(statement);
        }

        result.Interval = result.Interval.Merge(lexer.CurrentInterval());
        return result;
    }

    private DeclarationStatement ParseVariableDeclaration(bool allowInitializer = true)
    {
        var id = lexer.NextIdentifier();
        string name = id.Name;
        lexer.NextSymbol(":");
        var type = ParseType();

        Expression initializer = null;
        if (allowInitializer && lexer.NextSymbol("=", false) != null)
            initializer = ParseExpression();

        DeclarationStatement result = new(id.Interval, type);
        result.AddVariable(name, initializer);
        return result;
    }

    private void ParseFunctionDeclaration()
    {
        bool isExtern = lexer.NextKeyword("esterna", false) != null;

        var id = lexer.NextIdentifier();
        string name = id.Name;
        var f = unity.DeclareFunction(declaringType, name, id.Interval, isExtern) ?? throw new CompilerException(id.Interval, $"Função '{name}' já declarada.");

        lexer.NextSymbol("(");
        if (lexer.NextSymbol(")", false) == null)
        {
            ParseParamsDeclaration(f);
            lexer.NextSymbol(")");
        }

        if (lexer.NextSymbol(":", false) != null)
        {
            var type = ParseType();
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
        var id = lexer.NextIdentifier();
        string name = id.Name;
        var st = unity.DeclareStruct(name, id.Interval) ?? throw new CompilerException(id.Interval, $"Tipo '{name}' já declarado.");

        lexer.NextSymbol("{");
        if (lexer.NextSymbol("}", false) == null)
            ParseFieldsDeclaration(st);
    }

    private void ParseClassDeclaration()
    {
        // TODO : Implementar

        var id = lexer.NextIdentifier();
        string name = id.Name;
        var st = unity.DeclareClass(name, id.Interval) ?? throw new CompilerException(id.Interval, $"Tipo '{name}' já declarado.");

        lexer.NextSymbol("{");
        if (lexer.NextSymbol("}", false) == null)
            ParseFieldsDeclaration(st);
    }

    private void ParseInterfaceDeclaration()
    {
        // TODO : Implementar

        var id = lexer.NextIdentifier();
        string name = id.Name;
        var st = unity.DeclareClass(name, id.Interval) ?? throw new CompilerException(id.Interval, $"Tipo '{name}' já declarado.");

        lexer.NextSymbol("{");
        if (lexer.NextSymbol("}", false) == null)
            ParseFieldsDeclaration(st);
    }

    private void ParseEnumDeclaration()
    {
        // TODO : Implementar

        var id = lexer.NextIdentifier();
        string name = id.Name;
        var st = unity.DeclareStruct(name, id.Interval) ?? throw new CompilerException(id.Interval, $"Tipo '{name}' já declarado.");

        lexer.NextSymbol("{");
        if (lexer.NextSymbol("}", false) == null)
            ParseFieldsDeclaration(st);
    }

    private void ParseUnionDeclaration()
    {
        // TODO : Implementar

        var id = lexer.NextIdentifier();
        string name = id.Name;
        var st = unity.DeclareStruct(name, id.Interval) ?? throw new CompilerException(id.Interval, $"Tipo '{name}' já declarado.");

        lexer.NextSymbol("{");
        if (lexer.NextSymbol("}", false) == null)
            ParseFieldsDeclaration(st);
    }

    private void AddImport(SourceInterval interval, string unityName)
    {
        var r = unity.AddImport(unityName, out _);
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
        var kw = lexer.NextKeyword(false);
        if (kw != null)
        {
            switch (kw.Value)
            {
                case "usando":
                {
                    var id = lexer.NextIdentifier();
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
                    var declaration = ParseVariableDeclaration(false);
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

                case "classe":
                    ParseClassDeclaration();
                    return true;

                case "interface":
                    ParseInterfaceDeclaration();
                    return true;

                case "enum":
                    ParseEnumDeclaration();
                    return true;

                case "união":
                    ParseUnionDeclaration();
                    return true;
            }

            lexer.PreviusToken();
        }

        var start = lexer.NextSymbol("{", false);
        if (start != null)
        {
            var f = unity.DeclareFunction(null, "@main", start.Interval, false);
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
        var oldUnity = unity;
        var oldLexer = lexer;

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

            var id = lexer.NextIdentifier();
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

            var token = lexer.NextToken();
            if (token != null)
                throw new CompilerException(token.Interval, $"Fim do arquivo esperado mas {token} encontrado.");

            globalVariableOffset += unity.GlobalVariableSize;
        }

        var result = unity;
        lexer = oldLexer;
        unity = oldUnity;
        return result;
    }

#pragma warning disable IDE0051 // Remover membros privados não utilizados
    private void ParseCompilationUnity(CompilationUnity unity)
#pragma warning restore IDE0051 // Remover membros privados não utilizados
    {
        var oldUnity = this.unity;
        this.unity = unity;
        var oldLexer = lexer;
        unity.parsed = true;

        using (lexer = Lexer.CreateFromFile(unity.FileName))
        {
            if (lexer.NextKeyword("unidade", false) == null)
                throw new CompilerException(lexer.CurrentInterval(), "'unidade' esperado.");

            var id = lexer.NextIdentifier();
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

            var token = lexer.NextToken();
            if (token != null)
                throw new CompilerException(token.Interval, $"Fim do arquivo esperado mas {token} encontrado.");

            globalVariableOffset += unity.GlobalVariableSize;
        }

        lexer = oldLexer;
        this.unity = oldUnity;
    }
}