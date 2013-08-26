using System;
using System.IO;
using IronText.Framework;
using IronText.Logging;

namespace Samples
{
    public class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Argument should be path to the BNF file.");
                Console.ResetColor();
                return 1;
            }

            string path = args[0];
            if (!File.Exists(path))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("File does not exist.");
                Console.ResetColor();
                return 1;
            }

            string destBaseName = Path.GetFileNameWithoutExtension(path);
            
            using (var interpreter = new Interpreter<BnfLanguage>(Bnf2CSharpConverter.Create(destBaseName)))
            using (var input = new StreamReader(path))
            {
                interpreter.LogKind = LoggingKind.ConsoleOut;
                if (interpreter.Parse(input, path))
                {
                    Console.WriteLine("Conversion succeeded!");
                }
                else
                {
                    Console.WriteLine(
                        "============= {0} errors, {1} warnings ===========", 
                        interpreter.ErrorCount,
                        interpreter.WarningCount);
                }
            }

            return 0;
        }
    }
}
