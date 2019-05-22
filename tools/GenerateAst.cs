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
                "Binary   : Expression left, Token @operator, Expression right",
                "Grouping : Expression expression",
                "Literal  : object value",
                "Unary    : Token @operator, Expression right"
            });
        }

        // Generate Expression file with namespace, interface class, and all Expressions
        private static void DefineAst(string outputDir, string baseName, List<string> types)
        {
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(outputDir, $"{baseName}.cs")))
            {
                outputFile.WriteLine("namespace min {");

                outputFile.WriteLine("public abstract class Expression");
                outputFile.WriteLine("{");

                // Add a base accept method
                outputFile.WriteLine("public abstract T Accept<T>(IVisitor<T> visitor);");
                outputFile.WriteLine();

                DefineVisitor(outputFile, baseName, types);

                foreach (string type in types)
                {
                    string className = type.Split(":")[0].Trim();
                    string fields = type.Split(":")[1].Trim();
                    DefineType(outputFile, baseName, className, fields);
                }

                outputFile.WriteLine("}");

                outputFile.WriteLine("}");
            }
        }

        // Generate one Expression struct + fields + ctor
        private static void DefineType(StreamWriter outputFile, string baseName, string className, string fields)
        {
            outputFile.WriteLine();
            outputFile.WriteLine($"public class {className} : Expression");
            outputFile.WriteLine("{");

            string[] fieldList = fields.Split(", ");

            // Fields
            foreach (string field in fieldList)
            {
                outputFile.WriteLine($"public readonly {field};");
            }

            Console.WriteLine();

            // Constructor
            outputFile.WriteLine($"public {className} ({fields})");
            outputFile.WriteLine("{");

            foreach (string field in fieldList)
            {
                string name = field.Split(" ")[1];
                outputFile.WriteLine($"this.{name} = {name};");
            }

            outputFile.WriteLine("}");

            // Visitor pattern
            outputFile.WriteLine();
            outputFile.WriteLine("public override T Accept<T>(IVisitor<T> visitor)");
            outputFile.WriteLine("{");
            outputFile.WriteLine($"return visitor.Visit{className}(this);");
            outputFile.WriteLine("}");

            outputFile.WriteLine("}");
        }

        // Generate visitor interface to use Expressions
        private static void DefineVisitor(StreamWriter outputFile, string baseName, List<string> types)
        {
            outputFile.WriteLine("public interface IVisitor<T>");
            outputFile.WriteLine("{");

            foreach (string type in types)
            {
                string typeName = type.Split(":")[0].Trim();
                outputFile.WriteLine($"T Visit{typeName} ({typeName} {baseName.ToLower()});");
            }

            outputFile.WriteLine("}");
        }
    }
}