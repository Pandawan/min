using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace min.Debug
{
    // TODO: Continue ASTPrinter here https://craftinginterpreters.com/representing-code.html#a-(not-very)-pretty-printer
    public class AstPrinter : IExpressionVisitor<string>
    {
        public string Print(IExpression expression)
        {
            return expression.Accept(this);
        }

        public string VisitBinaryExpression(BinaryExpression expression)
        {
            return Parenthesize(expression.op.lexeme, expression.left, expression.right);
        }

        public string VisitCallExpression(CallExpression expression)
        {
            List<IExpression> expressions = new List<IExpression>();
            expressions.Add(expression.callee);
            expressions.AddRange(expression.arguments);
            return Parenthesize("call", expressions.ToArray());
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

        public string VisitLogicalExpression(LogicalExpression expression)
        {
            return Parenthesize(expression.op.lexeme, expression.left, expression.right);
        }

        public string VisitUnaryExpression(UnaryExpression expression)
        {
            return Parenthesize(expression.op.lexeme, expression.right);
        }

        public string VisitTernaryExpression(TernaryExpression expression)
        {
            return Parenthesize("conditional", expression.conditional, expression.thenExpression, expression.elseExpression);
        }

        public string VisitVariableExpression(VariableExpression expression)
        {
            return Parenthesize("read", expression);
        }

        public string VisitAssignExpression(AssignExpression expression)
        {
            return Parenthesize($"= {expression.name.lexeme}", expression.value);
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