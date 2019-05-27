using System.Collections.Generic;

namespace min
{
    /// <summary>
    /// Represents any object that can be called in Min. (ie. function, class constructor, etc.)
    /// </summary>
    public interface ICallable
    {
        object Call(Interpreter interpreter, IExpression expression, List<object> arguments);

        int Arity();
    }
}