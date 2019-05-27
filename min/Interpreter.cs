using System;
using System.Collections.Generic;
using min.lib;

namespace min
{
    public class Interpreter : IExpressionVisitor<object>, IStatementVisitor<object>
    {
        /// <summary>
        /// Fixed reference to the global environment.
        /// </summary>
        public readonly Environment globals = new Environment();

        /// <summary>
        /// Current environment of the interpreter.
        /// </summary>
        private Environment environment;

        public Interpreter()
        {
            // Add all the native library functions/objects to the global environment
            NativeLibrary.Initialize(globals);
            environment = globals;
        }

        public void Interpret(List<IStatement> statements, bool isRepl)
        {
            try
            {
                // If REPL & statement is an expression, print it to REPL (without having to do print)
                if (isRepl && statements.Count == 1 && statements[0] is ExpressionStatement)
                {
                    ExpressionStatement statement = (ExpressionStatement)statements[0];
                    object value = Evaluate(statement.expression);
                    Console.WriteLine(Stringify(value));
                }
                else
                {
                    foreach (IStatement statement in statements)
                    {
                        Execute(statement);
                    }
                }
            }
            catch (RuntimeError error)
            {
                Min.RuntimeError(error);
            }
        }

        #region Statements
        /**
         * Note: IStatementVisitor uses the type "object" instead of "void" because void is not allowed in C#
         * To remedy this issue, simply return null in all Visit_Statement methods
         */


        public object VisitExpressionStatement(ExpressionStatement statement)
        {
            Evaluate(statement.expression);
            return null;
        }

        public object VisitFunctionStatement(FunctionStatement statement)
        {
            // Use interpreter's current environment to allow for functions inside functions (closure)
            MinFunction function = new MinFunction(statement, environment);
            environment.Define(statement.name, function);
            return null;
        }

        public object VisitIfStatement(IfStatement statement)
        {
            if (IsTruthy(Evaluate(statement.condition)))
            {
                Execute(statement.thenBranch);
            }
            else if (statement.elseBranch != null)
            {
                Execute(statement.elseBranch);
            }
            return null;
        }

        public object VisitPrintStatement(PrintStatement statement)
        {
            object value = Evaluate(statement.expression);
            Console.WriteLine(Stringify(value));
            return null;
        }

        public object VisitReturnStatement(ReturnStatement statement)
        {
            object value = null;
            if (statement.value != null) value = Evaluate(statement.value);
            throw new Return(value);
        }

        public object VisitLetStatement(LetStatement statement)
        {
            // Default value of a variable is null
            object value = null;

            if (statement.initializer != null)
            {
                value = Evaluate(statement.initializer);
            }

            environment.Define(statement.name, value);

            return null;
        }

        public object VisitWhileStatement(WhileStatement statement)
        {
            while (IsTruthy(Evaluate(statement.condition)))
            {
                Execute(statement.body);
            }
            return null;
        }

        public object VisitBlockStatement(BlockStatement statement)
        {
            // Execute this block and pass it a new environment with the current one to enclose it.
            ExecuteBlock(statement.statements, new Environment(environment));
            return null;
        }

        private void Execute(IStatement statement)
        {
            statement.Accept(this);
        }

        public void ExecuteBlock(List<IStatement> statements, Environment environment)
        {
            // Keep a copy of the current environment before replacing it
            Environment previous = this.environment;
            try
            {
                this.environment = environment;

                // Run through all of the statements with the new environment
                foreach (IStatement statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                // Restore the previous environment even if an exception is thrown
                this.environment = previous;
            }
        }

        #endregion

        #region Expressions


        public object VisitLiteralExpression(LiteralExpression expressions)
        {
            return expressions.value;
        }

        public object VisitLogicalExpression(LogicalExpression expression)
        {
            object left = Evaluate(expression.left);

            if (expression.op.type == TokenType.OR)
            {
                // OR short circuit, true if the first one is true
                if (IsTruthy(left)) return left;
            }
            else
            {
                // AND short circuit, false if the first one is false
                if (IsTruthy(left) == false) return left;
            }

            return Evaluate(expression.right);
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
                case TokenType.COMMA: return right;
            }

            return null;
        }


        public object VisitCallExpression(CallExpression expression)
        {
            object callee = Evaluate(expression.callee);

            List<object> arguments = new List<object>();
            foreach (IExpression argument in expression.arguments)
            {
                arguments.Add(Evaluate(argument));
            }

            // Incorrect call, the calle cannot be called (ie. "some string"() )
            if (callee is ICallable == false)
            {
                throw new RuntimeError(expression.paren, "Can only call functions and classes.");
            }

            ICallable function = (ICallable)callee;

            // Not correct number of arguments
            if (arguments.Count != function.Arity())
            {
                // TODO: Might want to make it like JavaScript where it uses null when not enough, and discards when too many
                throw new RuntimeError(expression.paren, $"Expected {function.Arity()} arguments but got {arguments.Count}.");
            }

            return function.Call(this, expression, arguments);
        }

        public object VisitTernaryExpression(TernaryExpression expression)
        {
            object condition = Evaluate(expression.conditional);
            object thenExpression = Evaluate(expression.thenExpression);
            object elseExpression = Evaluate(expression.elseExpression);

            if (IsTruthy(condition)) return thenExpression;
            else return elseExpression;
        }


        public object VisitVariableExpression(VariableExpression expression)
        {
            return environment.Get(expression.name);
        }

        public object VisitAssignExpression(AssignExpression expression)
        {
            object value = Evaluate(expression.value);

            environment.Assign(expression.name, value);

            return value;
        }

        private object Evaluate(IExpression expression)
        {
            return expression.Accept(this);
        }

        #endregion

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
            if (value is string) return string.IsNullOrEmpty((string)value) == false;
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
        public string Stringify(object value)
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