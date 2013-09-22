using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronText.Diagnostics;
using IronText.Framework;
using NUnit.Framework;

namespace CSharpParser.Tests
{
    [TestFixture]
    [Explicit]
    public class BasicTest
    {
        [Test]
        public void Test()
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            string path = "Sample0.cs";
            using (var interp = new Interpreter<ICsGrammar>())
            {
                interp.LogKind = LoggingKind.ConsoleOut;
                TestLoad(timer, path, interp);

                //int tokenCount = interp.Scan(input, path).Count();
                //Console.WriteLine("token count = {0}", tokenCount);
                for (int i = 0; i != 3; ++i)
                {
                    timer.Reset();
                    using (var input = new StreamReader(path))
                    {
                        timer.Start();
                        interp.Recognize(input, path);
                        timer.Stop();

                        Console.WriteLine(
                            "{1}(1,1): message : Recognize time={0}sec",
                            timer.Elapsed.TotalSeconds,
                            path);

                        input.BaseStream.Seek(0, SeekOrigin.Begin);

                        timer.Start();
                        interp.Parse(input, path);
                        timer.Stop();

                        Console.WriteLine(
                            "{1}(1,1): message : Parse time={0}sec",
                            timer.Elapsed.TotalSeconds,
                            path);

                        timer.Start();
                        interp.BuildTree(input, path);
                        timer.Stop();

                        Console.WriteLine(
                            "{1}(1,1): message : BuildTree time={0}sec",
                            timer.Elapsed.TotalSeconds,
                            path);
                    }
#if false
                    var tree = interp.BuildTree(input, path);
                    using (IGraphView graph = new GvGraphView(Path.ChangeExtension(path, "sppf.gv")))
                    {
                        tree.WriteGraph(graph, interp.Grammar, false);
                    }
#endif
                }
            }
        }

        private static void TestLoad(Stopwatch timer, string path, Interpreter<ICsGrammar> interp)
        {
            interp.Parse("/*a*/[assembly:Foo]");
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
