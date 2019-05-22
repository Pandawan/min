namespace min
{
    public interface IExpression
    {
        T Accept<T>(IVisitor<T> visitor);
    }

    public interface IVisitor<T>
    {
        T VisitBinary(Binary expressions);
        T VisitGrouping(Grouping expressions);
        T VisitLiteral(Literal expressions);
        T VisitUnary(Unary expressions);
    }

    public struct Binary : IExpression
    {
        public readonly IExpression left;
        public readonly Token op;
        public readonly IExpression right;
        public Binary(IExpression left, Token op, IExpression right)
        {
            this.left = left;
            this.op = op;
            this.right = right;
        }

        public T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitBinary(this);
        }
    }

    public struct Grouping : IExpression
    {
        public readonly IExpression expression;
        public Grouping(IExpression expression)
        {
            this.expression = expression;
        }

        public T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitGrouping(this);
        }
    }

    public struct Literal : IExpression
    {
        public readonly object value;
        public Literal(object value)
        {
            this.value = value;
        }

        public T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitLiteral(this);
        }
    }

    public struct Unary : IExpression
    {
        public readonly Token op;
        public readonly IExpression right;
        public Unary(Token op, IExpression right)
        {
            this.op = op;
            this.right = right;
        }

        public T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitUnary(this);
        }
    }
}
