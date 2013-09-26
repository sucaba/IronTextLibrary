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
        public void BuildTest()
        {
            const string derivedDll = "CSharpParser.Derived.dll";
            if (File.Exists(derivedDll))
            {
                File.Delete(derivedDll);
            }

            var timer = new Stopwatch();
            timer.Start();

            var name = new LanguageName(typeof(ICsGrammar));
            var provider = new IronText.MetadataCompiler.LanguageDataProvider(name, false);
            IronText.Build.ResourceContext.Instance.LoadOrBuild(provider);
            var data = provider.Resource;
            timer.Stop();

            Console.WriteLine("Build time = {0}sec", timer.Elapsed.TotalSeconds);
        }

        [Test]
        public void BigTest()
        {
            string path = "Sample2.cs";
            Test(path);
        }

        [Test]
        public void ProfilableTest()
        {
            string path = "Sample0.cs";
            Test(path);
        }

        private void Test(string path)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            using (var interp = new Interpreter<ICsGrammar>())
            {
                interp.LogKind = LoggingKind.ConsoleOut;
                TestLoad(timer, path, interp);

                //int tokenCount = interp.Scan(input, path).Count();
                //Console.WriteLine("token count = {0}", tokenCount);
                for (int i = 0; i != 3; ++i)
                {
                    using (var input = new StreamReader(path))
                    {
                        input.BaseStream.Seek(0, SeekOrigin.Begin);
                        timer.Reset();
                        timer.Start();
                        input.ReadToEnd();
                        timer.Stop();

                        Console.WriteLine(
                            "{1}(1,1): message : Read      time={0}sec",
                            timer.Elapsed.TotalSeconds,
                            path);

                        input.BaseStream.Seek(0, SeekOrigin.Begin);
                        timer.Reset();
                        timer.Start();
                        interp.Scan(input, path).Count();
                        timer.Stop();

                        Console.WriteLine(
                            "{1}(1,1): message : Scan      time={0}sec",
                            timer.Elapsed.TotalSeconds,
                            path);

                        input.BaseStream.Seek(0, SeekOrigin.Begin);
                        timer.Reset();
                        timer.Start();
                        interp.Recognize(input, path);
                        timer.Stop();

                        Console.WriteLine(
                            "{1}(1,1): message : Recognize time={0}sec",
                            timer.Elapsed.TotalSeconds,
                            path);

                        input.BaseStream.Seek(0, SeekOrigin.Begin);
                        timer.Reset();
                        timer.Start();
                        interp.Parse(input, path);
                        timer.Stop();

                        Console.WriteLine(
                            "{1}(1,1): message : Parse     time={0}sec",
                            timer.Elapsed.TotalSeconds,
                            path);

                        input.BaseStream.Seek(0, SeekOrigin.Begin);
                        timer.Reset();
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
            interp.Parse("");
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
