using System;

namespace min
{
    class Program
    {
        private static int Main(string[] args)
        {
            Console.WriteLine("Min interpreter");

            if (args.Length > 1)
            {
                Console.WriteLine("Usage: min [script]");
                // Bad arguments exception (error code)
                return 64;
            }
            else if (args.Length == 1)
            {
                // Execute script
                return Min.RunFile(args[0]);
            }
            else
            {
                // Run prompt
                Min.RunPrompt();
                return 0;
            }
        }
    }
}