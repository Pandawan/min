namespace min
{
    public interface IExpressionI
    {

    }

    public struct BinaryExpressionI : IExpressionI
    {
        public readonly IExpressionI left;
        public readonly Token @operator;
        public readonly IExpressionI right;

        public BinaryExpressionI(IExpressionI left, Token @operator, IExpressionI right)
        {
            this.left = left;
            this.@operator = @operator;
            this.right = right;
        }
    }
}