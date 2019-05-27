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
        T VisitFunctionStatement(FunctionStatement statement);
        T VisitIfStatement(IfStatement statement);
        T VisitPrintStatement(PrintStatement statement);
        T VisitReturnStatement(ReturnStatement statement);
        T VisitLetStatement(LetStatement statement);
        T VisitWhileStatement(WhileStatement statement);
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

    public struct FunctionStatement : IStatement
    {
        public readonly Token name;
        public readonly List<Token> parameters;
        public readonly List<IStatement> body;

        public FunctionStatement(Token name, List<Token> parameters, List<IStatement> body)
        {
            this.name = name;
            this.parameters = parameters;
            this.body = body;
        }

        public T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitFunctionStatement(this);
        }
    }

    public struct IfStatement : IStatement
    {
        public readonly IExpression condition;
        public readonly IStatement thenBranch;
        public readonly IStatement elseBranch;

        public IfStatement(IExpression condition, IStatement thenBranch, IStatement elseBranch)
        {
            this.condition = condition;
            this.thenBranch = thenBranch;
            this.elseBranch = elseBranch;
        }

        public T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitIfStatement(this);
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

    public struct ReturnStatement : IStatement
    {
        public readonly Token keyword;
        public readonly IExpression value;

        public ReturnStatement(Token keyword, IExpression value)
        {
            this.keyword = keyword;
            this.value = value;
        }

        public T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitReturnStatement(this);
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

    public struct WhileStatement : IStatement
    {
        public readonly IExpression condition;
        public readonly IStatement body;

        public WhileStatement(IExpression condition, IStatement body)
        {
            this.condition = condition;
            this.body = body;
        }

        public T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitWhileStatement(this);
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
