using System;
using System.Collections.Generic;

namespace min
{
    public class Interpreter : IVisitor<object>
    {

        public void Interpret(IExpression expression)
        {
            try
            {
                object value = Evaluate(expression);
                Console.WriteLine(Stringify(value));
            }
            catch (RuntimeError error)
            {
                Min.RuntimeError(error);
            }
        }

        public object VisitLiteralExpression(LiteralExpression expressions)
        {
            return expressions.value;
        }

        public object VisitGroupingExpression(GroupingExpression expression)
        {
            return Evaluate(expression.expression);
        }

        public object VisitUnaryExpression(UnaryExpression expression)
        {
            object right = Evaluate(expression.right);

            switch (expression.op.type)
            {
                case TokenType.MINUS:
                    CheckNumberOperand(expression.op, right);
                    return -(double)right;
                case TokenType.BANG:
                    return !IsTruthy(right);
            }

            return null;
        }

        public object VisitBinaryExpression(BinaryExpression expression)
        {
            object left = Evaluate(expression.left);
            object right = Evaluate(expression.right);

            switch (expression.op.type)
            {

                case TokenType.MINUS:
                    CheckNumberOperand(expression.op, left, right);
                    return (double)left - (double)right;
                case TokenType.PLUS:
                    // Adding numbers
                    if (left is double && right is double)
                    {
                        return (double)left + (double)right;
                    }
                    // Concatenating strings
                    else if (left is string || right is string)
                    {
                        return Stringify(left) + Stringify(right);
                    }
                    throw new RuntimeError(expression.op, "Addition operation not supported for operands.");
                case TokenType.SLASH:
                    CheckNumberOperand(expression.op, left, right);
                    if ((double)right == 0) throw new RuntimeError(expression.op, "Cannot divide by zero.");
                    return (double)left / (double)right;
                case TokenType.STAR:
                    CheckNumberOperand(expression.op, left, right);
                    return (double)left * (double)right;
                case TokenType.GREATER:
                    CheckNumberOperand(expression.op, left, right);
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    CheckNumberOperand(expression.op, left, right);
                    return (double)left >= (double)right;
                case TokenType.LESS:
                    CheckNumberOperand(expression.op, left, right);
                    return (double)left < (double)right;
                case TokenType.LESS_EQUAL:
                    CheckNumberOperand(expression.op, left, right);
                    return (double)left <= (double)right;
                case TokenType.BANG_EQUAL: return !IsEqual(left, right);
                case TokenType.EQUAL_EQUAL: return IsEqual(left, right);
            }

            return null;
        }

        public object VisitTernaryExpression(TernaryExpression expression)
        {
            object condition = Evaluate(expression.conditional);
            object thenExpression = Evaluate(expression.thenExpression);
            object elseExpression = Evaluate(expression.elseExpression);

            if (IsTruthy(condition)) return thenExpression;
            else return elseExpression;
        }

        private object Evaluate(IExpression expression)
        {
            return expression.Accept(this);
        }

        /// <summary>
        /// Whether or not the given value is truthy.
        /// </summary>
        /// <param name="value">The value to test for.</param>
        /// <returns>A boolean representation of the value.</returns>
        private bool IsTruthy(object value)
        {
            // Null is falsy
            if (value == null) return false;
            // String is falsy if null or empty
            if (value is string) return string.IsNullOrEmpty((string)value);
            // Bool is already truthy
            if (value is bool) return (bool)value;
            // Empty arrays are falsy
            if (value.GetType().IsArray) return ((object[])value).Length != 0;
            // 0 doubles are falsy
            if (value is double) return (double)value != 0;

            return true;
        }

        /// <summary>
        /// Checks whether or not two given values are equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private bool IsEqual(object left, object right)
        {
            if (left == null && right == null) return true;
            if (left == null) return false;

            // TODO: Should objects of different types be equal? (i.e. false == 0 -> true)
            return left.Equals(right);
        }

        private void CheckNumberOperand(Token op, object operand)
        {
            if (operand is double) return;
            throw new RuntimeError(op, "Operand must be a number.");
        }

        private void CheckNumberOperand(Token op, object left, object right)
        {
            if (left is double && right is double) return;
            throw new RuntimeError(op, "Operands must be numbers.");
        }

        /// <summary>
        /// Convert a given value to a string.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The string representation of that value.</returns>
        private string Stringify(object value)
        {
            if (value == null) return "null";
            if (value is bool) return (bool)value ? "true" : "false";
            if (value.GetType().IsArray)
            {
                object[] arr = (object[])value;
                if (arr.Length == 0) return "[]";
                else string.Join(", ", arr);
            }
            if (value is double)
            {
                string num = value.ToString();
                return num.EndsWith(".0") ? num.Substring(0, num.Length - 2) : num;
            }

            return value.ToString();
        }
    }
}