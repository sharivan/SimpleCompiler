using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using compiler.types;

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

    public class DeclarationStatement : InitializerStatement
    {
        private AbstractType type;
        private List<Tuple<string, Expression>> vars;

        public AbstractType Type
        {
            get => type;

            internal set => type = value;
        }

        public int VariableCount => vars.Count;

        public Tuple<string, Expression> this[int index]
        {
            get => vars[index];

            internal set => vars[index] = value;
        }

        internal DeclarationStatement(SourceInterval interval, AbstractType type) : base(interval)
        {
            this.type = type;

            vars = new List<Tuple<string, Expression>>();
        }

        internal void AddVariable(string name, Expression initializer = null)
        {
            vars.Add(new Tuple<string, Expression>(name, initializer));
        }

        internal void Resolve()
        {
            AbstractType.Resolve(ref type);
        }
    }

    public class ExpressionStatement : InitializerStatement
    {
        private Expression expression;

        public Expression Expression
        {
            get => expression;

            internal set => expression = value;
        }

        internal ExpressionStatement(SourceInterval interval, Expression expression) : base(interval)
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

            internal set => expression = value;
        }

        internal ReturnStatement(SourceInterval interval, Expression expression = null) : base(interval)
        {
            this.expression = expression;
        }
    }

    public class BreakStatement : Statement
    {
        internal BreakStatement(SourceInterval interval) : base(interval)
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

            internal set => expressions[index] = value;
        }

        internal PrintStatement(SourceInterval interval, bool lineBreak) : base(interval)
        {
            this.lineBreak = lineBreak;

            expressions = new List<Expression>();
        }

        internal void AddExpression(Expression expression)
        {
            expressions.Add(expression);
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

            internal set => expression = value;
        }

        public Statement ThenStatement
        {
            get => thenStatement;

            internal set => thenStatement = value;
        }

        public Statement ElseStatement
        {
            get => elseStatement;

            internal set => elseStatement = value;
        }

        internal IfStatement(SourceInterval interval, Expression expression, Statement thenStatement, Statement elseStatement = null) : base(interval)
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

            internal set => expression = value;
        }

        public Statement Statement
        {
            get => statement;

            internal set => statement = value;
        }

        internal WhileStatement(SourceInterval interval, Expression expression, Statement statement) : base(interval)
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

            internal set => expression = value;
        }

        public Statement Statement
        {
            get => statement;

            internal set => statement = value;
        }

        internal DoStatement(SourceInterval interval, Expression expression, Statement statement) : base(interval)
        {
            this.expression = expression;
            this.statement = statement;
        }
    }

    public class ForStatement : Statement
    {
        private List<InitializerStatement> initializers;
        private Expression expression;
        private List<Expression> updaters;
        private Statement statement;

        public int InitializerCount => initializers.Count;

        public Expression Expression
        {
            get => expression;

            internal set => expression = value;
        }

        public int UpdaterCount => updaters.Count;

        public Statement Statement
        {
            get => statement;

            internal set => statement = value;
        }

        internal ForStatement(SourceInterval interval, Expression expression = null) : base(interval)
        {
            this.expression = expression;

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

    public class BlockStatement : Statement
    {
        private List<Statement> statements;

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
    }
}
