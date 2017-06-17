#if !DEBUG 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using IronText.Framework;
using System.IO;
using System.Diagnostics;
using IronText.Runtime;
using IronText.Testing;
using IronText.Tests.Extensions;

namespace IronText.Tests.Performance
{
    [TestFixture(Category="Performance")]
    public class PublicPerformance
    {
        [Test]
        public void _0_TestLalr1()
        {
            int count = 1000000;
            TestLalr1("LALR1", count, 3);
        }

        [Test]
        [Explicit]
        public void TestLalr1Profilable()
        {
            int count = 1000;
            TestLalr1("Profilable LALR1", count, 1);
        }

        [Test]
        public void _1_TestLalr1AsGlr()
        {
            int count = 1000000;
            TestLalr1(typeof(Lalr1PerfLangAsGlr), "GLR not ambiguous", count, 3);
        }

        [Test]
        [Explicit]
        public void TestLalr1AsGlrProfilable()
        {
            int count = 1000;
            TestLalr1(typeof(Lalr1PerfLangAsGlr), "Profilable LALR1(forceGlr)", count, 1);
        }

        [Test]
        [Explicit]
        public void TestLalr1AsGlrProfilable2()
        {
            int count = 10;
            TestLalr1(typeof(Lalr1PerfLangAsGlr), "Profilable2 LALR1(forceGlr)", count, 1);
        }

        private static void TestLalr1(string title, int count, int trialCount = 1)
        {
            TestLalr1(typeof(Lalr1PerfLang), title, count, trialCount);
        }

        private static void TestLalr1(Type langDef, string title, int count, int trialCount = 1)
        {
            const string path = "EFa.test";
            using (var testFile = new StreamWriter(path))
            {
                testFile.Write("a");

                while (count-- != 0)
                {
                    testFile.Write("+a");
                }
            }

            Benchmarks(path, title, langDef, trialCount);
        }

        [Test]
        public void _2_TestGlr()
        {
            int count = 200;
            TestGlr("GLR ambiguous", count, 3);
        }

        [Test]
        [Explicit]
        public void TestGlrProfilableNoTree()
        {
            int count = 100;
            TestGlr("Profilable GLR (actions)", count, 1, BenchFlags.ParseActions);
        }

        [Test]
        [Explicit]
        public void TestGlrProfilableTree()
        {
            int count = 100;
            TestGlr("Profilable GLR (tree)", count, 1, BenchFlags.ParseTree);
        }

        private static void TestGlr(string title, int count, int trialCount, BenchFlags flags = BenchFlags.All)
        {
            const string path = "EEa.test";
            using (var testFile = new StreamWriter(path))
            {
                testFile.Write("a");
                "+a".Times(count, testFile);
            }

            Benchmarks(path, title, typeof(GlrPerfLang), trialCount, flags: flags);
        }

        private static void Log(string format, params object[] args)
        {
            Trace.WriteLine(string.Format(format, args));
        }

        enum BenchFlags
        {
            None         = 0x00,
            ParseTree    = 0x01,
            ParseActions = 0x02,
            Recognize    = 0x04,
            All          = ParseTree | ParseActions | Recognize
        }

        private static void Benchmarks(
            string path,
            string title,
            Type   langDef,
            int    trialCount = 1,
            BenchFlags flags = BenchFlags.All)
        {
            var timer = new Stopwatch();

            timer.Start();
            var lang = Language.Get(langDef);
            using (var interp = new Interpreter(lang))
            {
                interp.Parse("a+a");
            }
            timer.Stop();

            Log("--------------- " + title + " ---------------");
            Log("heatup time  = {0}", timer.Elapsed);

            timer.Reset();
            using (var testFile = new StreamReader(path))
            {
                timer.Start();
                testFile.ReadToEnd();
                timer.Stop();
                Log("read time    = {0}", timer.Elapsed);
            }

            using (var interp = new Interpreter(lang))
            {
                for (int trial = 0; trial != trialCount; ++trial)
                {
                    timer.Reset();
                    using (var testFile = new StreamReader(path))
                    {
                        timer.Start();

                        int count = interp.Scan(testFile, path).Count();

                        timer.Stop();
                        Log("scan time    = {0} tokenCount = {1}", timer.Elapsed, count);
                    }
                }

                // Recognize
                if ((flags & BenchFlags.Recognize) == BenchFlags.Recognize)
                {
                    for (int trial = 0; trial != trialCount; ++trial)
                    {
                        timer.Reset();
                        using (var testFile = new StreamReader(path))
                        {
                            timer.Start();
                            bool success = interp.Recognize(testFile, path);
                            timer.Stop();
                            Assert.IsTrue(success);
                            Log("parse (recognize) = {0}", timer.Elapsed);
                        }
                    }
                }

                // Parse actions
                if ((flags & BenchFlags.ParseActions) == BenchFlags.ParseActions)
                {
                    for (int trial = 0; trial != trialCount; ++trial)
                    {
                        timer.Reset();
                        using (var testFile = new StreamReader(path))
                        {
                            timer.Start();
                            bool success = interp.Parse(testFile, path);
                            timer.Stop();
                            Assert.IsTrue(success);
                            Log("parse (actions) = {0}", timer.Elapsed);
                        }
                    }
                }

                // Parse tree
                if ((flags & BenchFlags.ParseTree) == BenchFlags.ParseTree)
                {
                    for (int trial = 0; trial != trialCount; ++trial)
                    {
                        timer.Reset();
                        using (var testFile = new StreamReader(path))
                        {
                            timer.Start();
                            interp.BuildTree(testFile, path);
                            timer.Stop();
                            Log("parse (tree) = {0}", timer.Elapsed);
                        }
                    }
                }
            }
        }

        [Language]
        public class Lalr1PerfLang
        {
            [Produce]
            public void Start(E e) { }

            [Produce(null, "+", null)]
            public E Sum(E e, F f) { return null; }

            [Produce]
            public E F(F f) { return null; }

            [Produce("a")]
            public F Fa() { return null; }

            [Produce("(", null, ")")]
            public F FE(E e) { return null; }
        }

        [Language(RuntimeOptions.ForceNonDeterministic)]
        [DescribeParserStateMachine("Lalr1PerfLangAsGlr.gram")]
        public class Lalr1PerfLangAsGlr : Lalr1PerfLang
        {
        }

        [Language(RuntimeOptions.AllowNonDeterministic)]
        [ParserGraph(nameof(GlrPerfLang) + ".gv")]
        [DescribeParserStateMachine(nameof(GlrPerfLang) + ".info")]
        public class GlrPerfLang
        {
            [Produce]
            public void Start(E e) { }

            [Produce(null, "+", null)]
            public E Sum(E e1, E e2) { return null; }

            [Produce("a")]
            public E Ea() { return null; }
        }

        public interface E {}
        public interface F {}

        private static string TextTimes(string pattern, int times)
        {
            using (var writer = new StringWriter())
            {
                TextTimes(writer, pattern, times);

                return writer.ToString();
            }
        }

        private static void TextTimes(StringWriter writer, string pattern, int times)
        {
            for (int count = times; count-- != 0;)
            {
                writer.Write(pattern);
            }
        }
    }
}

#endif
