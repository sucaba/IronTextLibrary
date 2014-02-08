using System;
using IronText.Framework;
using IronText.Logging;
using IronText.Runtime;

namespace Calculator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var context = new Calculator();

            using (var interp = new Interpreter<Calculator>(context))
            {
                interp.LoggingKind = LoggingKind.Console;

                while (true)
                {
                    Console.Write("calc>");

                    var expr = Console.ReadLine();
                    try
                    {
                        interp.Parse(expr);
                        if (context.Done)
                        {
                            break;
                        }

                        if (interp.ErrorCount == 0)
                        {
                            Console.WriteLine(context.Result);
                        }
                    }
                    catch (Exception error)
                    {
                        while (error.InnerException != null)
                        {
                            error = error.InnerException;
                        }

                        Console.WriteLine(error.Message);
                    }
                }
            }
        }
    }
}
