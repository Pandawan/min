using System.Collections.Generic;

namespace min
{
    public class MinFunction : ICallable
    {
        private readonly FunctionStatement declaration;
        private readonly Environment closure;

        public MinFunction(FunctionStatement declaration, Environment closure)
        {
            this.declaration = declaration;
            this.closure = closure;
        }

        public int Arity()
        {
            return declaration.parameters.Count;
        }

        public object Call(Interpreter interpreter, IExpression callExpression, List<object> arguments)
        {
            // Need to call the function in its own environment to allow for recursive functions
            Environment environment = new Environment(closure);

            for (int i = 0; i < declaration.parameters.Count; i++)
            {
                environment.Define(declaration.parameters[i].lexeme, arguments[i]);
            }

            try
            {
                interpreter.ExecuteBlock(declaration.body, environment);
            }
            // If there is an early return using a return statement, catch and return it.
            catch (Return returnValue)
            {
                return returnValue.value;
            }

            return null;
        }

        public override string ToString()
        {
            return $"<fn {declaration.name.lexeme}>";
        }
    }
}
