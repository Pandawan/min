using System;
using System.Text;

namespace min.Debug
{
    // TODO: Continue ASTPrinter here https://craftinginterpreters.com/representing-code.html#a-(not-very)-pretty-printer
    public class AstPrinter : IVisitor<string>
    {
        public string Print(IExpression expression)
        {
            return expression.Accept(this);
        }

        public string VisitBinaryExpression(BinaryExpression expression)
        {
            return Parenthesize(expression.op.lexeme, expression.left, expression.right);
        }

        public string VisitGroupingExpression(GroupingExpression expression)
        {
            return Parenthesize("group", expression.expression);
        }

        public string VisitLiteralExpression(LiteralExpression expression)
        {
            if (expression.value == null) return "null";
            return expression.value.ToString();
        }

        public string VisitUnaryExpression(UnaryExpression expression)
        {
            return Parenthesize(expression.op.lexeme, expression.right);
        }

        /// <summary>
        /// Format the given expressions into a string with parentheses around them.
        /// </summary>
        /// <param name="name">The name of the group.</param>
        /// <param name="expressions">The expressions to parenthesize.</param>
        /// <returns>A formatted string.</returns>
        private string Parenthesize(string name, params IExpression[] expressions)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("(").Append(name);
            foreach (IExpression expression in expressions)
            {
                builder.Append(" ").Append(expression.Accept(this));
            }
            builder.Append(")");

            return builder.ToString();
        }
    }
}