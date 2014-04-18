using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IronText.Algorithm;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Runtime;
using NUnit.Framework;

namespace IronText.Tests.Framework
{
    [TestFixture]
    public class InheritanceTest
    {
        [Test]
        [Explicit]
        public void Dbg()
        {
            Console.WriteLine(Language.Get(typeof(MainLang)).Grammar);
        }

        [Test]
        public void MainLanguageModuleProvidesRuleMethods()
        {
            var target = new MainLang();
            Language.Parse(target, "mainLang");
        }

        [Test]
        public void InheritedLanguageModuleProvidesRuleMethods()
        {
            var target = new MainLang();
            Language.Parse(target, "baseLang");
        }

        [Test]
        public void InheritedLanguageModuleProvidesStaticRuleMethods()
        {
            var target = new MainLang();
            Language.Parse(target, "staticBaseLang");
        }

        [Test]
        public void DemandRuleInMainLang()
        {
            var target = new MainLang();
            Language.Parse(target, "mainLang loop1 loop1 end");
        }

        [Test]
        public void DemandRuleInBase()
        {
            var target = new MainLang();
            Language.Parse(target, "mainLang loop1Base loop1Base endBase");
        }

        [Test]
        [Explicit]
        public void ShowMetadata()
        {
            var roots = MetadataParser.EnumerateAndBind(typeof(Loop1Syntax));
            Debug.WriteLine("Roots:");
            foreach (var meta in roots)
            {
                Debug.WriteLine(meta.ToString());
            }

            metaToIndent.Clear();

            Debug.WriteLine("");
            Debug.WriteLine("All:");
            var all = Graph.AllVertexes(roots, GetIndentedChildren).ToArray();
            foreach (var meta in all)
            {
                Debug.WriteLine(new string('\t', metaToIndent[meta]) + meta.ToString());
            }
        }

        private readonly Dictionary<ICilMetadata, int> metaToIndent = new Dictionary<ICilMetadata, int>();

        private IEnumerable<ICilMetadata> GetIndentedChildren(ICilMetadata parent)
        {
            int indent;
            if (!metaToIndent.TryGetValue(parent, out indent))
            {
                indent = 0;
            }

            ++indent;
            foreach (var child in parent.GetChildren())
            {
                metaToIndent[child] = indent;
                yield return child;
            }
        }

        public interface ResultOf<T> { }

        [Vocabulary]
        public class BaseLang
        {
            [Produce]
            public void Result(ResultOf<BaseLang> main) { }

            [Produce("baseLang")]
            public ResultOf<BaseLang> ParseBaseLang() { return null; }

            [Produce("staticBaseLang")]
            public static ResultOf<BaseLang> StaticBaseLang() { return null; }
        }

        [Language]
        [GrammarDocument("InheritanceTest.gram")]
        [ScannerDocument("InheritanceTest.scan")]
        [DescribeParserStateMachine("InheritanceTest.info")]
        public class MainLang : BaseLang
        {
            [Produce]
            public void Result(ResultOf<MainLang> main) { }

            [Produce("mainLang")]
            public ResultOf<MainLang> ParseMainLang() { return null; }

            [Produce("mainLang")]
            public ResultOf<MainLang> ParseMainLang(ResultOf<Loop1Syntax> loop1Result) { return null; }

            [Produce]
            public Loop1Syntax SeedLoop1Syntax() { return new Loop1Syntax(); }

            [Match("blank+")]
            public static void Blank() { }
        }

        [Demand]
        public abstract class Loop1SyntaxBase
        {
            [Produce("loop1Base")]
            public Loop1SyntaxBase ParseBase() { return this; }

            [Produce("endBase")]
            public ResultOf<Loop1Syntax> EndBase() { return null; }
        }

        [Demand]
        public class Loop1Syntax : Loop1SyntaxBase // inheritance should add new rules with modified context-token
        {
            [Produce("loop1")]
            public Loop1Syntax Parse() { return this; }

            [Produce("end")]
            public ResultOf<Loop1Syntax> End() { return null; }
        }
    }
}
