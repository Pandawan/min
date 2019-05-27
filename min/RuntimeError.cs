using System;

namespace min
{
    public class RuntimeError : Exception
    {
        public readonly Token token;

        public RuntimeError(Token token, string message) : base(message)
        {
            this.token = token;
        }
    }

    /// <summary>
    /// Used for return statements to quickly get out of a function call.
    /// </summary>
    public class Return : Exception
    {
        public readonly object value;
        public Return(object value) : base(null)
        {
            this.value = value;
        }
    }
}