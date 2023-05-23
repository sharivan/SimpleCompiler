using System.Collections;
using System.Collections.Generic;

using compiler.types;

namespace compiler;

public abstract class Statement
{
    public SourceInterval Interval
    {
        get;
        internal set;
    }

    protected Statement(SourceInterval interval)
    {
        Interval = interval;
    }
}

public class EmptyStatement : Statement
{
    internal EmptyStatement(SourceInterval interval) : base(interval)
    {
    }
}

public abstract class InitializerStatement : Statement
{
    protected InitializerStatement(SourceInterval interval) : base(interval)
    {
    }
}

public class DeclarationStatement : InitializerStatement, IEnumerable<(string, Expression)>
{
    private AbstractType type;
    private readonly List<(string, Expression)> vars;

    public AbstractType Type
    {
        get => type;
        internal set => type = value;
    }

    public int VariableCount => vars.Count;

    public (string, Expression) this[int index]
    {
        get => vars[index];
        internal set => vars[index] = value;
    }

    internal DeclarationStatement(SourceInterval interval, AbstractType type) : base(interval)
    {
        this.type = type;

        vars = new List<(string, Expression)>();
    }

    internal void AddVariable(string name, Expression initializer = null)
    {
        vars.Add((name, initializer));
    }

    internal void Resolve()
    {
        AbstractType.Resolve(ref type);
    }

    public IEnumerator<(string, Expression)> GetEnumerator()
    {
        return vars.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return vars.GetEnumerator();
    }
}

public class ExpressionStatement : InitializerStatement
{
    public Expression Expression
    {
        get; internal set;
    }

    internal ExpressionStatement(SourceInterval interval, Expression expression) : base(interval)
    {
        Expression = expression;
    }
}

public class ReturnStatement : Statement
{
    public Expression Expression
    {
        get; internal set;
    }

    internal ReturnStatement(SourceInterval interval, Expression expression = null) : base(interval)
    {
        Expression = expression;
    }
}

public class BreakStatement : Statement
{
    internal BreakStatement(SourceInterval interval) : base(interval)
    {
    }
}

public class ReadStatement : Statement, IEnumerable<Expression>
{
    private readonly List<Expression> expressions;

    public int ExpressionCount => expressions.Count;

    public Expression this[int index]
    {
        get => expressions[index];

        internal set => expressions[index] = value;
    }

    internal ReadStatement(SourceInterval interval) : base(interval)
    {
        expressions = new List<Expression>();
    }

    internal void AddExpression(Expression expression)
    {
        expressions.Add(expression);
    }

    public IEnumerator<Expression> GetEnumerator()
    {
        return expressions.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return expressions.GetEnumerator();
    }
}

public class PrintStatement : Statement, IEnumerable<Expression>
{
    private readonly List<Expression> expressions;

    public bool LineBreak
    {
        get;
    }

    public int ExpressionCount => expressions.Count;

    public Expression this[int index]
    {
        get => expressions[index];
        internal set => expressions[index] = value;
    }

    internal PrintStatement(SourceInterval interval, bool lineBreak) : base(interval)
    {
        LineBreak = lineBreak;

        expressions = new List<Expression>();
    }

    internal void AddExpression(Expression expression)
    {
        expressions.Add(expression);
    }

    public IEnumerator<Expression> GetEnumerator()
    {
        return expressions.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return expressions.GetEnumerator();
    }
}

public class IfStatement : Statement
{
    public Expression Expression
    {
        get; internal set;
    }

    public Statement ThenStatement
    {
        get; internal set;
    }

    public Statement ElseStatement
    {
        get; internal set;
    }

    internal IfStatement(SourceInterval interval, Expression expression, Statement thenStatement, Statement elseStatement = null) : base(interval)
    {
        Expression = expression;
        ThenStatement = thenStatement;
        ElseStatement = elseStatement;
    }
}

public class WhileStatement : Statement
{
    public Expression Expression
    {
        get; internal set;
    }

    public Statement Statement
    {
        get; internal set;
    }

    internal WhileStatement(SourceInterval interval, Expression expression, Statement statement) : base(interval)
    {
        Expression = expression;
        Statement = statement;
    }
}

public class DoStatement : Statement
{
    public Expression Expression
    {
        get; internal set;
    }

    public Statement Statement
    {
        get; internal set;
    }

    internal DoStatement(SourceInterval interval, Expression expression, Statement statement) : base(interval)
    {
        Expression = expression;
        Statement = statement;
    }
}

public class ForStatement : Statement
{
    private readonly List<InitializerStatement> initializers;
    private readonly List<Expression> updaters;

    public IEnumerable<InitializerStatement> Initializers => initializers;

    public IEnumerable<Expression> Updaters => updaters;

    public int InitializerCount => initializers.Count;

    public Expression Expression
    {
        get; internal set;
    }

    public int UpdaterCount => updaters.Count;

    public Statement Statement
    {
        get; internal set;
    }

    internal ForStatement(SourceInterval interval, Expression expression = null) : base(interval)
    {
        Expression = expression;

        initializers = new List<InitializerStatement>();
        updaters = new List<Expression>();
    }

    internal void AddInitializer(InitializerStatement initializer)
    {
        initializers.Add(initializer);
    }

    public InitializerStatement GetInitializer(int index)
    {
        return initializers[index];
    }

    internal void SetInitializer(int index, InitializerStatement value)
    {
        initializers[index] = value;
    }

    internal void AddUpdater(Expression updater)
    {
        updaters.Add(updater);
    }

    public Expression GetUpdater(int index)
    {
        return updaters[index];
    }

    internal void SetUpdater(int index, Expression value)
    {
        updaters[index] = value;
    }
}

public class BlockStatement : Statement, IEnumerable<Statement>
{
    private readonly List<Statement> statements;

    public int StatementCount => statements.Count;

    public Statement this[int index]
    {
        get => statements[index];

        internal set => statements[index] = value;
    }

    internal BlockStatement(SourceInterval interval) : base(interval)
    {
        statements = new List<Statement>();
    }

    internal void AddStatement(Statement statement)
    {
        statements.Add(statement);
    }

    public IEnumerator<Statement> GetEnumerator()
    {
        return statements.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return statements.GetEnumerator();
    }
}
