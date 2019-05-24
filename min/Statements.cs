using System.Collections.Generic;

namespace min
{
    public interface IStatement
    {
        T Accept<T>(IStatementVisitor<T> visitor);
    }

    public interface IStatementVisitor<T>
    {
        T VisitExpressionStatement(ExpressionStatement statement);
        T VisitPrintStatement(PrintStatement statement);
        T VisitLetStatement(LetStatement statement);
        T VisitBlockStatement(BlockStatement statement);
    }

    public struct ExpressionStatement : IStatement
    {
        public readonly IExpression expression;

        public ExpressionStatement(IExpression expression)
        {
            this.expression = expression;
        }

        public T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitExpressionStatement(this);
        }
    }

    public struct PrintStatement : IStatement
    {
        public readonly IExpression expression;

        public PrintStatement(IExpression expression)
        {
            this.expression = expression;
        }

        public T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitPrintStatement(this);
        }
    }

    public struct LetStatement : IStatement
    {
        public readonly Token name;
        public readonly IExpression initializer;

        public LetStatement(Token name, IExpression initializer)
        {
            this.name = name;
            this.initializer = initializer;
        }

        public T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitLetStatement(this);
        }
    }

    public struct BlockStatement : IStatement
    {
        public readonly List<IStatement> statements;

        public BlockStatement(List<IStatement> statements)
        {
            this.statements = statements;
        }

        public T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitBlockStatement(this);
        }
    }
}
