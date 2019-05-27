using System;
using System.Collections.Generic;

namespace min.lib
{
    public static class NativeLibrary
    {
        public static void Initialize(Environment globals)
        {
            globals.Define("print", new Print());
            globals.Define("clock", new Clock());
            globals.Define("input", new Input());
        }
    }

    /// <summary>
    /// Print the given string to the console.
    /// </summary>
    public struct Print : ICallable
    {
        public int Arity()
        {
            return 1;
        }

        public object Call(Interpreter interpreter, IExpression expression, List<object> arguments)
        {
            foreach (object obj in arguments)
            {
                Console.WriteLine(interpreter.Stringify(obj));
            }

            return null;
        }

        public override string ToString()
        {
            return "<native fn>";
        }
    }

    /// <summary>
    /// Get current time in milliseconds.
    /// </summary>
    public struct Clock : ICallable
    {
        public int Arity()
        {
            return 0;
        }

        public object Call(Interpreter interpreter, IExpression expression, List<object> arguments)
        {
            return (double)DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        public override string ToString()
        {
            return "<native fn>";
        }
    }


    /// <summary>
    /// Read string from input.
    /// </summary>
    public struct Input : ICallable
    {
        public int Arity()
        {
            return 0;
        }

        public object Call(Interpreter interpreter, IExpression expression, List<object> arguments)
        {
            string input = null;

            try
            {
                input = Console.ReadLine();
            }
            catch
            {
                CallExpression callExpression = (CallExpression)expression;
                throw new RuntimeError(callExpression.paren, "Could not read from input.");
            }

            return input;
        }

        public override string ToString()
        {
            return "<native fn>";
        }
    }
}