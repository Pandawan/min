using System;
using System.IO;
using System.Collections.Generic;

namespace min
{
    class Min
    {
        /// <summary>
        /// Keep a static version so that it can keep global state when used in REPL.
        /// </summary>
        private static readonly Interpreter interpreter = new Interpreter();

        /// <summary>
        /// Whether or not the program encountered an error during compilation.
        /// </summary>
        private static bool hadError = false;

        /// <summary>
        /// Whether or not the program encountered an error during execution.
        /// </summary>
        private static bool hadRuntimeError = false;

        #region Running

        /// <summary>
        /// Runs the file at the given path.
        /// </summary>
        /// <param name="path">The path of the file to run.</param>
        /// <returns>An exit code.</returns>
        public static int RunFile(string path)
        {
            string source = File.ReadAllText(path);
            Run(source, false);

            if (hadError) return 65;
            if (hadRuntimeError) return 70;

            return 0;
        }

        /// <summary>
        /// Starts a console prompt to run in REPL mode.
        /// </summary>
        public static void RunPrompt()
        {
            while (true)
            {
                Console.Write("> ");
                string input = Console.ReadLine();

                // Exit ends the program
                if (input.Trim() == "exit")
                {
                    return;
                }

                Run(input, true);

                // If there was an error, don't crash the entire program
                hadError = false;
                // TODO: Add "exit" to call Exit()
            }
        }

        /// <summary>
        /// Run the source code provided.
        /// </summary>
        /// <param name="source">The source code to run.</param>
        /// <param name="isRepl">Whether or not this is running as REPL</param>
        private static void Run(string source, bool isRepl)
        {
            // Scanner/Lexer, read tokens
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.ScanTokens();

            // Parser, group tokens into expressions
            Parser parser = new Parser(tokens);
            List<IStatement> statements = parser.Parse();

            // Stop if there was a syntax error.
            if (hadError) return;

            interpreter.Interpret(statements, isRepl);
        }

        #endregion

        #region Errors

        /// <summary>
        /// Report a simple error.
        /// </summary>
        /// <param name="line">The line at which the program had an error.</param>
        /// <param name="message">The message explaining the error.</param>
        public static void Error(int line, string message)
        {
            // TODO: Add better error reporting with ^----- here or other reporting
            Report(line, "", message);
        }

        /// <summary>
        /// Report a token (parsing) error.
        /// </summary>
        /// <param name="token">The token at which the program had an error.</param>
        /// <param name="message">The message explaining the error.</param>
        public static void Error(Token token, string message)
        {
            if (token.type == TokenType.EOF)
            {
                Report(token.line, " at end", message);
            }
            else
            {
                Report(token.line, $" at '{token.lexeme}'", message);
            }
        }

        public static void RuntimeError(RuntimeError error)
        {
            Console.WriteLine($"[line {error.token.line}] {error.Message}");
            hadRuntimeError = true;
        }

        /// <summary>
        /// Actually reports an error through the console.
        /// </summary>
        /// <param name="line">The line at which the program had an error.</param>
        /// <param name="where">Where in the line the program had an error</param>
        /// <param name="message"></param>
        public static void Report(int line, string where, string message)
        {
            Console.WriteLine($"[line {line}] Error{where}: {message}");
            hadError = true;
        }

        #endregion
    }
}
