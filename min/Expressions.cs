namespace min
{
    public interface IExpression
    {
        T Accept<T>(IVisitor<T> visitor);
    }

    public interface IVisitor<T>
    {
        T VisitBinaryExpression(BinaryExpression expressions);
        T VisitGroupingExpression(GroupingExpression expressions);
        T VisitLiteralExpression(LiteralExpression expressions);
        T VisitUnaryExpression(UnaryExpression expressions);
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

        public T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitBinaryExpression(this);
        }
    }

    public struct GroupingExpression : IExpression
    {
        public readonly IExpression expression;
        public GroupingExpression(IExpression expression)
        {
            this.expression = expression;
        }

        public T Accept<T>(IVisitor<T> visitor)
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

        public T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitLiteralExpression(this);
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

        public T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitUnaryExpression(this);
        }
    }
}
