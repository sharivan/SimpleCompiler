using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler
{
    public abstract class Statement
    {
        private SourceInterval interval;

        public SourceInterval Interval => interval;

        protected Statement(SourceInterval interval)
        {
            this.interval = interval;
        }
    }

    public class EmptyStatement : Statement
    {
        public EmptyStatement(SourceInterval interval) : base(interval)
        {
        }
    }

    public class ExpressionStatement : Statement
    {
        private Expression expression;

        public Expression Expression
        {
            get => expression;

            set => expression = value;
        }

        public ExpressionStatement(SourceInterval interval, Expression expression) : base(interval)
        {
            this.expression = expression;
        }
    }

    public class ReturnStatement : Statement
    {
        private Expression expression;

        public Expression Expression
        {
            get => expression;

            set => expression = value;
        }

        public ReturnStatement(SourceInterval interval, Expression expression = null) : base(interval)
        {
            this.expression = expression;
        }
    }

    public class BreakStatement : Statement
    {
        public BreakStatement(SourceInterval interval) : base(interval)
        {
        }
    }

    public class ReadStatement : Statement
    {
        private List<Expression> expressions;

        public int ExpressionCount => expressions.Count;

        public Expression this[int index]
        {
            get => expressions[index];

            set => expressions[index] = value;
        }

        public ReadStatement(SourceInterval interval) : base(interval)
        {
            expressions = new List<Expression>();
        }

        public void AddExpression(Expression expression)
        {
            expressions.Add(expression);
        }
    }

    public class PrintStatement : Statement
    {
        private bool lineBreak;
        private List<Expression> expressions;

        public bool LineBreak => lineBreak;

        public int ExpressionCount => expressions.Count;

        public Expression this[int index]
        {
            get => expressions[index];

            set => expressions[index] = value;
        }

        public PrintStatement(SourceInterval interval, bool lineBreak) : base(interval)
        {
            this.lineBreak = lineBreak;

            expressions = new List<Expression>();
        }

        public void AddExpression(Expression expression)
        {
            expressions.Add(expression);
        }
    }

    public class DeclarationStatement : Statement
    {
        private AbstractType type;
        private List<Tuple<string, Expression>> vars;

        public AbstractType Type
        {
            get => type;

            set => type = value;
        }

        public int VariableCount => vars.Count;

        public Tuple<string, Expression> this[int index]
        {
            get => vars[index];

            set => vars[index] = value;
        }

        public DeclarationStatement(SourceInterval interval, AbstractType type) : base(interval)
        {
            this.type = type;

            vars = new List<Tuple<string, Expression>>();
        }

        public void AddVariable(string name, Expression initializer = null)
        {
            vars.Add(new Tuple<string, Expression>(name, initializer));
        }
    }

    public class IfStatement : Statement
    {
        private Expression expression;
        private Statement thenStatement;
        private Statement elseStatement;

        public Expression Expression
        {
            get => expression;

            set => expression = value;
        }

        public Statement ThenStatement
        {
            get => thenStatement;

            set => thenStatement = value;
        }

        public Statement ElseStatement
        {
            get => elseStatement;

            set => elseStatement = value;
        }

        public IfStatement(SourceInterval interval, Expression expression, Statement thenStatement, Statement elseStatement = null) : base(interval)
        {
            this.expression = expression;
            this.thenStatement = thenStatement;
            this.elseStatement = elseStatement;
        }
    }

    public class WhileStatement : Statement
    {
        private Expression expression;
        private Statement statement;

        public Expression Expression
        {
            get => expression;

            set => expression = value;
        }

        public Statement Statement
        {
            get => statement;

            set => statement = value;
        }

        public WhileStatement(SourceInterval interval, Expression expression, Statement statement) : base(interval)
        {
            this.expression = expression;
            this.statement = statement;
        }
    }

    public class DoStatement : Statement
    {
        private Expression expression;
        private Statement statement;

        public Expression Expression
        {
            get => expression;

            set => expression = value;
        }

        public Statement Statement
        {
            get => statement;

            set => statement = value;
        }

        public DoStatement(SourceInterval interval, Expression expression, Statement statement) : base(interval)
        {
            this.expression = expression;
            this.statement = statement;
        }
    }

    public class ForStatement : Statement
    {
        private List<Expression> initializers;
        private Expression expression;
        private List<Expression> updaters;
        private Statement statement;

        public int InitializerCount => initializers.Count;

        public Expression Expression
        {
            get => expression;

            set => expression = value;
        }

        public int UpdaterCount => updaters.Count;

        public Statement Statement
        {
            get => statement;

            set => statement = value;
        }

        public ForStatement(SourceInterval interval, Expression expression = null) : base(interval)
        {
            this.expression = expression;

            initializers = new List<Expression>();
            updaters = new List<Expression>();
        }

        public void AddInitializer(Expression initializer)
        {
            initializers.Add(initializer);
        }

        public Expression GetInitializer(int index)
        {
            return initializers[index];
        }

        public void SetInitializer(int index, Expression value)
        {
            initializers[index] = value;
        }

        public void AddUpdater(Expression updater)
        {
            updaters.Add(updater);
        }

        public Expression GetUpdater(int index)
        {
            return updaters[index];
        }

        public void SetUpdater(int index, Expression value)
        {
            updaters[index] = value;
        }
    }

    public class BlockStatement : Statement
    {
        private List<Statement> statements;

        public int StatementCount => statements.Count;

        public Statement this[int index]
        {
            get => statements[index];

            set => statements[index] = value;
        }

        public BlockStatement(SourceInterval interval) : base(interval)
        {
            statements = new List<Statement>();
        }

        public void AddStatement(Statement statement)
        {
            statements.Add(statement);
        }
    }
}
