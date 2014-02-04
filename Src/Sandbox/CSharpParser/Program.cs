using System;
using System.Diagnostics;
using System.IO;
using IronText.Logging;
using IronText.Runtime;

namespace CSharpParser
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            var path = args[0];
            using (var interp = new Interpreter<ICsGrammar>())
            {
                TestLoad(timer, path, interp);

                //int tokenCount = interp.Scan(input, path).Count();
                //Console.WriteLine("token count = {0}", tokenCount);
                for (int i = 0; i != 10; ++i)
                {
                    timer.Reset();
                    using (var input = new StreamReader(path))
                    {
                        timer.Start();
                        interp.Parse(input, path);
                        timer.Stop();
                    }

                    Console.WriteLine(
                        "{1}(1,1): message : Parsing time={0}sec",
                        timer.Elapsed.TotalSeconds,
                        path);
                }
            }
        }

        private static void TestLoad(Stopwatch timer, string path, Interpreter<ICsGrammar> interp)
        {
            interp.LoggingKind = LoggingKind.Console;
            interp.Parse("/*a*/[a]");
            timer.Stop();
            if (timer.ElapsedMilliseconds > 1000)
            {
                Console.WriteLine(
                    "{1}(1,1): warning : Language loading is too slow time={0}sec",
                    timer.Elapsed.TotalSeconds,
                    path);
            }
        }
    }
}
