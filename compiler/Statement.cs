using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler
{
    public abstract class Statement
    {
    }

    public class EmptyStatement : Statement
    {
        public EmptyStatement()
        {
        }
    }

    public class ExpressionStatement : Statement
    {
        private Expression expression;

        public Expression Expression
        {
            get
            {
                return expression;
            }

            set
            {
                expression = value;
            }
        }

        public ExpressionStatement(Expression expression)
        {
            this.expression = expression;
        }
    }

    public class ReturnStatement : Statement
    {
        private Expression expression;

        public Expression Expression
        {
            get
            {
                return expression;
            }

            set
            {
                expression = value;
            }
        }

        public ReturnStatement(Expression expression = null)
        {
            this.expression = expression;
        }
    }

    public class BreakStatement : Statement
    {
    }

    public class ReadStatement : Statement
    {
        private List<Expression> expressions;

        public int ExpressionCount
        {
            get
            {
                return expressions.Count;
            }
        }

        public Expression this[int index]
        {
            get
            {
                return expressions[index];
            }

            set
            {
                expressions[index] = value;
            }
        }

        public ReadStatement()
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
        private List<Expression> expressions;

        public int ExpressionCount
        {
            get
            {
                return expressions.Count;
            }
        }

        public Expression this[int index]
        {
            get
            {
                return expressions[index];
            }

            set
            {
                expressions[index] = value;
            }
        }

        public PrintStatement()
        {
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
            get
            {
                return type;
            }

            set
            {
                type = value;
            }
        }

        public int VariableCount
        {
            get
            {
                return vars.Count;
            }
        }

        public Tuple<string, Expression> this[int index]
        {
            get
            {
                return vars[index];
            }

            set
            {
                vars[index] = value;
            }
        }

        public DeclarationStatement(AbstractType type)
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
            get
            {
                return expression;
            }

            set
            {
                expression = value;
            }
        }

        public Statement ThenStatement
        {
            get
            {
                return thenStatement;
            }

            set
            {
                thenStatement = value;
            }
        }

        public Statement ElseStatement
        {
            get
            {
                return elseStatement;
            }

            set
            {
                elseStatement = value;
            }
        }

        public IfStatement(Expression expression, Statement thenStatement, Statement elseStatement = null)
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
            get
            {
                return expression;
            }

            set
            {
                expression = value;
            }
        }

        public Statement Statement
        {
            get
            {
                return statement;
            }

            set
            {
                statement = value;
            }
        }

        public WhileStatement(Expression expression, Statement statement)
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
            get
            {
                return expression;
            }

            set
            {
                expression = value;
            }
        }

        public Statement Statement
        {
            get
            {
                return statement;
            }

            set
            {
                statement = value;
            }
        }

        public DoStatement(Expression expression, Statement statement)
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

        public int InitializerCount
        {
            get
            {
                return initializers.Count;
            }
        }

        public Expression Expression
        {
            get
            {
                return expression;
            }

            set
            {
                expression = value;
            }
        }

        public int UpdaterCount
        {
            get
            {
                return updaters.Count;
            }
        }

        public Statement Statement
        {
            get
            {
                return statement;
            }

            set
            {
                statement = value;
            }
        }

        public ForStatement(Expression expression = null)
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

        public int StatementCount
        {
            get
            {
                return statements.Count;
            }
        }

        public Statement this[int index]
        {
            get
            {
                return statements[index];
            }

            set
            {
                statements[index] = value;
            }
        }

        public BlockStatement()
        {
            statements = new List<Statement>();
        }

        public void AddStatement(Statement statement)
        {
            statements.Add(statement);
        }
    }
}
