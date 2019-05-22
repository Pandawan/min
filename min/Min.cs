using System;
using System.IO;
using System.Collections.Generic;

namespace min
{
    class Min
    {
        /// <summary>
        /// Whether or not the program encountered an error during compilation, execution, etc.
        /// </summary>
        private static bool hadError = false;

        private static int Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: min [script]");
                // Bad arguments exception (error code)
                return 64;
            }
            else if (args.Length == 1)
            {
                // Execute script
                RunFile(args[0]);
                return 0;
            }
            else
            {
                // Run prompt
                RunPrompt();
                return 0;
            }
        }

        #region Running

        /// <summary>
        /// Runs the file at the given path.
        /// </summary>
        /// <param name="path">The path of the file to run.</param>
        /// <returns>An exit code.</returns>
        private static void RunFile(string path)
        {
            string source = File.ReadAllText(path);
            Run(source);

            if (hadError) Exit(65);
        }

        /// <summary>
        /// Starts a console prompt to run in REPL mode.
        /// </summary>
        private static void RunPrompt()
        {
            while (true)
            {
                Console.Write("> ");
                Run(Console.ReadLine());

                // If there was an error, don't crash the entire program
                hadError = false;
                // TODO: Add "exit" to call Exit()
            }
        }

        /// <summary>
        /// Run the source code provided.
        /// </summary>
        /// <param name="source">The source code to run.</param>
        private static void Run(string source)
        {
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.ScanTokens();

            foreach (Token token in tokens)
            {
                Console.WriteLine(token);
            }
        }

        /// <summary>
        /// Exit the program.
        /// </summary>
        /// <param name="exitCode">The exit code to return, default is 0.</param>
        private static void Exit(int exitCode = 0)
        {
            // TODO: Add a list of hardcoded static/const exit codes
            // 64 = Incorrect command usage
            // 65 = Incorrect input data
            Environment.Exit(exitCode);
        }

        #endregion

        /// <summary>
        /// Report an error.
        /// </summary>
        /// <param name="line">The line at which the program had an error.</param>
        /// <param name="message">The message explaining the error.</param>
        public static void Error(int line, string message)
        {
            // TODO: Add better error reporting with ^----- here or other reporting
            Console.WriteLine($"[line {line}] Error: {message}");
            hadError = true;
        }
    }
}
