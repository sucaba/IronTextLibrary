using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IronText.Algorithm;
using IronText.Extensibility;
using IronText.Framework;
using NUnit.Framework;

namespace IronText.Tests.Framework
{
    [TestFixture]
    public class InheritanceTest
    {
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
        public void ThisAsTokenInMainLangMethods()
        {
            var target = new MainLang();
            Language.Parse(target, "mainLang loop1 loop1 end");
        }

        [Test]
        public void BaseThisAsTokenClassProvidesRuleMethods()
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

        private readonly Dictionary<ILanguageMetadata, int> metaToIndent = new Dictionary<ILanguageMetadata, int>();

        private IEnumerable<ILanguageMetadata> GetIndentedChildren(ILanguageMetadata parent)
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
            [Parse]
            public void Result(ResultOf<BaseLang> main) { }

            [Parse("baseLang")]
            public ResultOf<BaseLang> ParseBaseLang() { return null; }
        }

        [Language]
        [GrammarDocument("InheritanceTest.gram")]
        [ScannerDocument("InheritanceTest.scan")]
        [DescribeParserStateMachine("InheritanceTest.info")]
        public class MainLang : BaseLang
        {
            [Parse]
            public void Result(ResultOf<MainLang> main) { }

            [Parse("mainLang")]
            public ResultOf<MainLang> ParseMainLang() { return null; }

            [Parse("mainLang")]
            public ResultOf<MainLang> ParseMainLang(ResultOf<Loop1Syntax> loop1Result) { return null; }

            [Parse]
            public Loop1Syntax SeedLoop1Syntax() { return new Loop1Syntax(); }

            [Scan("blank+")]
            public static void Blank() { }
        }

        [Demand]
        public abstract class Loop1SyntaxBase
        {
            [Parse("loop1Base")]
            public Loop1SyntaxBase ParseBase() { return this; }

            [Parse("endBase")]
            public ResultOf<Loop1Syntax> EndBase() { return null; }
        }

        [Demand]
        public class Loop1Syntax : Loop1SyntaxBase // inheritance should add new rules with modified context-token
        {
            [Parse("loop1")]
            public Loop1Syntax Parse() { return this; }

            [Parse("end")]
            public ResultOf<Loop1Syntax> End() { return null; }
        }
    }
}
