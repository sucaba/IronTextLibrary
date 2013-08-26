using System;
using IronText.Framework;
using IronText.Logging;

namespace Samples
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var context = new NestedCommentSyntax();
            using (var interp = new Interpreter<NestedCommentSyntax>(context))
            {
                interp.LogKind = LoggingKind.ConsoleOut;

                const string text = "/*/* /* foo *//* middle */ * / * */ bar */";
                bool success = interp.Parse(text);
                if (success)
                {
                    Console.WriteLine("Comment text is:");
                    Console.WriteLine(context.Result);
                }
            }
        }
    }
}
