using System;
using System.IO;
using System.Collections.Generic;

namespace tools
{
    /// <summary>
    /// Simple script to generate an Expression.cs file with multiple IExpression structs.
    /// </summary>
    public class GenerateAst
    {
        public static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: generate_ast <output directory>");
                Environment.Exit(64);
            }

            string outputDir = args[0];



            DefineAst(outputDir, "Expression", new List<string>() {
                "BinaryExpression   : IExpression left, Token @operator, IExpression right",
                "GroupingExpression : IExpression expression",
                "LiteralExpression  : object value",
                "UnaryExpression    : Token @operator, IExpression right"
            });
        }

        private static void DefineAst(string outputDir, string baseName, List<string> types)
        {
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(outputDir, $"{baseName}.cs")))
            {
                outputFile.WriteLine("namespace min {");

                outputFile.WriteLine("public interface IExpression {}");

                foreach (string type in types)
                {
                    string className = type.Split(":")[0].Trim();
                    string fields = type.Split(":")[1].Trim();
                    DefineType(outputFile, baseName, className, fields);
                }

                outputFile.WriteLine("}");
            }
        }

        private static void DefineType(StreamWriter outputFile, string baseName, string className, string fields)
        {
            outputFile.WriteLine();
            outputFile.WriteLine($"public struct {className} : IExpression");
            outputFile.WriteLine("{");

            string[] fieldList = fields.Split(", ");

            foreach (string field in fieldList)
            {
                outputFile.WriteLine($"public readonly {field};");
            }

            outputFile.WriteLine($"public {className} ({fields})");
            outputFile.WriteLine("{");

            foreach (string field in fieldList)
            {
                string name = field.Split(" ")[1];
                outputFile.WriteLine($"this.{name} = {name};");
            }

            outputFile.WriteLine("}");

            outputFile.WriteLine("}");
        }
    }
}