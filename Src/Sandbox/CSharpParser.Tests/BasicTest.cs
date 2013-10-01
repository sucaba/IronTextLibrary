using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading;
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
        public void BigRecognizerTest()
        {
            string path = "Sample2.cs";
            Test(path, TestFlags.RunRecognizer, 3);
        }

        [Test]
        public void BigTest()
        {
            string path = "Sample2.cs";
            Test(path, TestFlags.All, 3);
        }

        [Test]
        public void ProfilableRecognizeTest()
        {
            string path = "Sample1.cs";
            Test(path, TestFlags.RunRecognizer, repeatCount:1);
        }

        private void Test(string path, TestFlags flags = TestFlags.All, int repeatCount = 3)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            using (var interp = new Interpreter<ICsGrammar>())
            {
                interp.LogKind = LoggingKind.ConsoleOut;
                TestLoad(timer, path, interp);

                using (var input = new StreamReader(path))
                {
                    for (int i = 0; i != repeatCount; ++i)
                    {
                        if ((flags & TestFlags.RunFileRead) == TestFlags.RunFileRead)
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
                        }

                        if ((flags & TestFlags.RunScanner) == TestFlags.RunScanner)
                        {
                            input.BaseStream.Seek(0, SeekOrigin.Begin);
                            timer.Reset();
                            timer.Start();
                            int tokenCount = interp.Scan(input, path).Count();
                            timer.Stop();

                            Console.WriteLine(
                                "{1}(1,1): message : Scan      time={0}sec // token count = {2}",
                                timer.Elapsed.TotalSeconds,
                                path,
                                tokenCount);
                        }

                        if ((flags & TestFlags.RunRecognizer) == TestFlags.RunRecognizer)
                        {
                            input.BaseStream.Seek(0, SeekOrigin.Begin);
                            timer.Reset();
                            timer.Start();
                            interp.Recognize(input, path);
                            timer.Stop();

                            Console.WriteLine(
                                "{1}(1,1): message : Recognize time={0}sec",
                                timer.Elapsed.TotalSeconds,
                                path);
                        }

                        if ((flags & TestFlags.RunParser) == TestFlags.RunParser)
                        {
                            input.BaseStream.Seek(0, SeekOrigin.Begin);
                            timer.Reset();
                            timer.Start();
                            interp.Parse(input, path);
                            timer.Stop();

                            Console.WriteLine(
                                "{1}(1,1): message : Parse     time={0}sec",
                                timer.Elapsed.TotalSeconds,
                                path);
                        }

                        if ((flags & TestFlags.RunSppfBuilder) == TestFlags.RunSppfBuilder)
                        {
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
                    }
                }
            }
        }

        private static void TestLoad(Stopwatch timer, string path, Interpreter<ICsGrammar> interp)
        {
            interp.Parse("[assembly:foo]");
            timer.Stop();
            if (timer.ElapsedMilliseconds > 1000)
            {
                Console.WriteLine(
                    "{1}(1,1): warning : Language loading is too slow time={0}sec",
                    timer.Elapsed.TotalSeconds,
                    path);
            }
        }

        enum TestFlags
        {
            RunFileRead     = 0x01,
            RunScanner      = 0x02,
            RunRecognizer   = 0x04,
            RunParser       = 0x08,
            RunSppfBuilder  = 0x10,
            All             = 0xff,
        }
    }
}
