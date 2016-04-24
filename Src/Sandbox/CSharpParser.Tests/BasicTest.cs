using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IronText.Algorithm;
using IronText.Diagnostics;
using IronText.Framework;
using IronText.Reflection;
using IronText.MetadataCompiler;
using NUnit.Framework;
using IronText.Logging;
using IronText.Runtime;
using IronText.Reflection.Managed;

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

            var name = new TypedLanguageSource(typeof(ICsGrammar));
            var provider = new IronText.MetadataCompiler.LanguageDataProvider(name, false);
            IronText.Build.ResourceContext.Instance.LoadOrBuild(provider);
            var data = provider.Resource;
            timer.Stop();

            // ShowReduceStates(data);
            //ShowSimilarStates(data);
            //ShowSimilarTokens(data);

            Console.WriteLine("Build time = {0}sec", timer.Elapsed.TotalSeconds);
        }

#if false
        private static void ShowSimilarStates(LanguageData data)
        {
            int stateCount = data.ParserStates.Length;
            var grammar = data.Grammar;
            int[] terms = (from s in grammar.Symbols where s.IsTerminal select s.Index).ToArray();
            var table = data.ParserActionTable;
            var unpartitioned = Enumerable.Range(0, stateCount).ToList();
            var categories = new List<List<int>>();

            while (unpartitioned.Count != 0)
            {
                int p = unpartitioned[0];
                unpartitioned.RemoveAt(0);
                var category = new List<int> { p };

                for (int j = 1; j < unpartitioned.Count;)
                {
                    int q = unpartitioned[j];

                    if (AreCompatibleStates(data, terms, table, p, q))
                    {
                        unpartitioned.RemoveAt(j);
                        category.Add(q);
                    }
                    else
                    {
                        ++j;
                    }
                }

                categories.Add(category);

                Console.WriteLine("{ " + string.Join(", ", category) + "}");
            }
        }
#endif

        private static bool AreCompatibleStates(LanguageData data, int[] terms, ITable<int> table, int p, int q)
        {
            foreach (int term in terms)
            {
                var pCell = table.Get(p, term);
                var qCell = table.Get(q, term);
                var pAction = ParserAction.Decode(pCell);
                var qAction = ParserAction.Decode(qCell);

                if (pCell != qCell 
                    && !HaveSameShifts(data, pAction, qAction))
                {
                    return false;
                }

                if (pAction.Kind == ParserActionKind.Conflict 
                    || qAction.Kind == ParserActionKind.Conflict)
                {
                    Console.WriteLine("Skipped conflicting {0}<->{1} states", p, q);
                    return false;
                }

                if (!HaveComptibleReductions(data, pAction, qAction))
                {
                    /*
                    Console.WriteLine(
                        "Skipped incompatible {0}<->{1} state reductions {2}<->{3}",
                        p, q,
                        pAction, qAction);
                    */

                    return false;
                }
            }

            return true;
        }

        private static bool HaveComptibleReductions(LanguageData data, ParserAction pAction, ParserAction qAction)
        {
            return 
                (
                    pAction.Kind != ParserActionKind.Reduce
                    && qAction.Kind != ParserActionKind.Reduce)
                ||
                (
                    pAction.Kind == ParserActionKind.Reduce
                    && qAction.Kind == ParserActionKind.Reduce
                    && pAction.ProductionId == qAction.ProductionId
                );
        }

        private static bool HaveSameShifts(LanguageData data, ParserAction x, ParserAction y)
        {
            var xShift = GetShift(data, x);
            var yShift = GetShift(data, y);
            return xShift == yShift;
        }

        private static ParserAction GetShift(LanguageData data, ParserAction x)
        {
            switch (x.Kind)
            {
                case ParserActionKind.Shift:
                case ParserActionKind.ShiftReduce:
                    return x;
                case ParserActionKind.Conflict:
                    int start = x.Value1;
                    int count = x.Value2;
                    int last = start + count;
                    for (; start != last; ++start)
                    {
                        var cAction = data.ParserConflictActionTable[start];
                        switch (cAction.Kind)
                        {
                            case ParserActionKind.Shift:
                            case ParserActionKind.ShiftReduce:
                                return cAction;
                        }
                    }

                    break;
            }

            return ParserAction.FailAction;
        }

#if false
        // Merging similar tokens (token equivalence classes) makes sense only 
        // after state compression
        private static void ShowSimilarTokens(LanguageData data)
        {
            var grammar = data.Grammar;

            int stateCount = data.ParserStates.Length;
            int startToken = PredefinedTokens.Count;
            int tokenCount = grammar.Symbols.Count;

            var table = data.ParserActionTable;
            var unpartitioned = Enumerable.Range(startToken, tokenCount - startToken).ToList();
            var categories = new List<List<int>>();

            while (unpartitioned.Count != 0)
            {
                int p = unpartitioned[0];
                unpartitioned.RemoveAt(0);
                var category = new List<int> { p };

                for (int j = 1; j < unpartitioned.Count;)
                {
                    int q = unpartitioned[j];

                    bool areSimilar = true;
                    for (int s = 0; s != stateCount; ++s)
                    {
                        var pCell = table.Get(s, p);
                        var qCell = table.Get(s, q);
                        if (pCell != qCell)
                        {
                            areSimilar = false;
                            break;
                        }
                    }

                    if (areSimilar)
                    {
                        unpartitioned.RemoveAt(j);
                        category.Add(q);
                    }
                    else
                    {
                        ++j;
                    }
                }

                categories.Add(category);

                Console.WriteLine("T{ " + string.Join(", ", category) + "}");
            }
        }
#endif

#if false
                    var pRules = new List<int>();
                    var qRules = new List<int>();
                    foreach (var term in terms)
                    {
                        var pCell = table.Get(p, term);
                        var qCell = table.Get(q, term);
                        var pAction = ParserAction.Decode(pCell);
                        var qAction = ParserAction.Decode(pCell);

                        if (pAction.Kind == ParserActionKind.Reduce)
                        {
                            pRules.Add(pAction.Rule);
                        }

                        if (qAction.Kind == ParserActionKind.Reduce)
                        {
                            qRules.Add(qAction.Rule);
                        }
                    }

                    if (pRules.Count != qRules.Count)
                    {
                        continue;
                    }

                    pRules.Sort();
                    qRules.Sort();
                    if (!Enumerable.SequenceEqual(pRules, qRules))
                    {
                        continue;
                    }
#endif

        private static bool IsTermShiftAction(LanguageData data, int cell)
        {
            var kind = ParserAction.GetKind(cell);
            switch (kind)
            {
                case ParserActionKind.Shift:
                case ParserActionKind.ShiftReduce:
                    return true;
                case ParserActionKind.Conflict:
                    var action = ParserAction.Decode(cell);
                    int start = action.Value1;
                    int count = action.Value2;
                    int last = start + count;
                    while (start != last)
                    {
                        ParserAction cAction = data.ParserConflictActionTable[start++];
                        switch (cAction.Kind)
                        {
                            case ParserActionKind.Shift:
                            case ParserActionKind.ShiftReduce:
                                return true;
                            default:
                                return false;
                        }
                    }
                    return false;
                default:
                    return false;
            }
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
                interp.LoggingKind = LoggingKind.Console;
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
