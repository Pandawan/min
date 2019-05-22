namespace min
{
    public interface IExpression { }

    public struct BinaryExpression : IExpression
    {
        public readonly IExpression left;
        public readonly Token @operator;
        public readonly IExpression right;
        public BinaryExpression(IExpression left, Token @operator, IExpression right)
        {
            this.left = left;
            this.@operator = @operator;
            this.right = right;
        }
    }

    public struct GroupingExpression : IExpression
    {
        public readonly IExpression expression;
        public GroupingExpression(IExpression expression)
        {
            this.expression = expression;
        }
    }

    public struct LiteralExpression : IExpression
    {
        public readonly object value;
        public LiteralExpression(object value)
        {
            this.value = value;
        }
    }

    public struct UnaryExpression : IExpression
    {
        public readonly Token @operator;
        public readonly IExpression right;
        public UnaryExpression(Token @operator, IExpression right)
        {
            this.@operator = @operator;
            this.right = right;
        }
    }
}
