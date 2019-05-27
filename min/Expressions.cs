using System.Collections.Generic;

namespace min
{
    public interface IExpression
    {
        T Accept<T>(IExpressionVisitor<T> visitor);
    }

    public interface IExpressionVisitor<T>
    {
        T VisitBinaryExpression(BinaryExpression expression);
        T VisitCallExpression(CallExpression expression);
        T VisitGroupingExpression(GroupingExpression expression);
        T VisitLiteralExpression(LiteralExpression expression);
        T VisitLogicalExpression(LogicalExpression expression);
        T VisitUnaryExpression(UnaryExpression expression);
        T VisitTernaryExpression(TernaryExpression expression);
        T VisitVariableExpression(VariableExpression expression);
        T VisitAssignExpression(AssignExpression expression);
    }

    public struct BinaryExpression : IExpression
    {
        public readonly IExpression left;
        public readonly Token op;
        public readonly IExpression right;

        public BinaryExpression(IExpression left, Token op, IExpression right)
        {
            this.left = left;
            this.op = op;
            this.right = right;
        }

        public T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitBinaryExpression(this);
        }
    }

    public struct CallExpression : IExpression
    {
        public readonly IExpression callee;
        public readonly Token paren;
        public readonly List<IExpression> arguments;

        public CallExpression(IExpression callee, Token paren, List<IExpression> arguments)
        {
            this.callee = callee;
            this.paren = paren;
            this.arguments = arguments;
        }

        public T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitCallExpression(this);
        }
    }

    public struct GroupingExpression : IExpression
    {
        public readonly IExpression expression;

        public GroupingExpression(IExpression expression)
        {
            this.expression = expression;
        }

        public T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitGroupingExpression(this);
        }
    }

    public struct LiteralExpression : IExpression
    {
        public readonly object value;

        public LiteralExpression(object value)
        {
            this.value = value;
        }

        public T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitLiteralExpression(this);
        }
    }

    public struct LogicalExpression : IExpression
    {
        public readonly IExpression left;
        public readonly Token op;
        public readonly IExpression right;

        public LogicalExpression(IExpression left, Token op, IExpression right)
        {
            this.left = left;
            this.op = op;
            this.right = right;
        }

        public T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitLogicalExpression(this);
        }
    }

    public struct UnaryExpression : IExpression
    {
        public readonly Token op;
        public readonly IExpression right;

        public UnaryExpression(Token op, IExpression right)
        {
            this.op = op;
            this.right = right;
        }

        public T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitUnaryExpression(this);
        }
    }

    public struct TernaryExpression : IExpression
    {
        public readonly IExpression conditional;
        public readonly IExpression thenExpression;
        public readonly IExpression elseExpression;

        public TernaryExpression(IExpression conditional, IExpression thenExpression, IExpression elseExpression)
        {
            this.conditional = conditional;
            this.thenExpression = thenExpression;
            this.elseExpression = elseExpression;
        }

        public T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitTernaryExpression(this);
        }
    }

    public struct VariableExpression : IExpression
    {
        public readonly Token name;

        public VariableExpression(Token name)
        {
            this.name = name;
        }

        public T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitVariableExpression(this);
        }
    }

    public struct AssignExpression : IExpression
    {
        public readonly Token name;
        public readonly IExpression value;

        public AssignExpression(Token name, IExpression value)
        {
            this.name = name;
            this.value = value;
        }

        public T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitAssignExpression(this);
        }
    }
}
