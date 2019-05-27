using System.Collections.Generic;

namespace min
{
    public class Environment
    {
        /// <summary>
        ///  Keep a reference to the parent environment to allow for block scopes.
        /// </summary>
        private readonly Environment enclosing;

        private readonly Dictionary<string, object> values = new Dictionary<string, object>();

        public Environment()
        {
            enclosing = null;
        }

        public Environment(Environment enclosing)
        {
            this.enclosing = enclosing;
        }

        public void Define(Token name, object value)
        {
            // Don't allow defining when the variable already exists
            if (values.ContainsKey(name.lexeme) == false)
            {
                values.Add(name.lexeme, value);
                return;
            }

            throw new RuntimeError(name, $"Identifier '{name.lexeme}' has already been declared.");
        }

        /// <summary>
        /// NOTE: ONLY USE IN NATIVE LIBRARY
        /// </summary>
        public void Define(string name, object value)
        {
            Token token = new Token(TokenType.NULL, name, null, 0);
            Define(token, value);
        }

        public object Get(Token name)
        {
            if (values.ContainsKey(name.lexeme))
            {
                return values[name.lexeme];
            }

            // If it couldn't find it in this scope, check in the enclosing one
            if (enclosing != null) return enclosing.Get(name);

            throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
        }

        public void Assign(Token name, object value)
        {
            // Don't allow assigning when the variable doesn't exist
            if (values.ContainsKey(name.lexeme))
            {
                values[name.lexeme] = value;
                return;
            }

            // If it couldn't find it in this scope, check in the enclosing one
            if (enclosing != null)
            {
                enclosing.Assign(name, value);
                return;
            }

            throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
        }
    }
}