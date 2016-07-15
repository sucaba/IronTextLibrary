using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework;
using IronText.Reflection;
using IronText.Runtime;
using NUnit.Framework;

namespace IronText.Tests.Framework
{
    [TestFixture]
    public class AmbTokenTest
    {
        [Test]
        public void DeterministicParserTest()
        {
            Assert.AreEqual(
                ParserRuntime.Deterministic,
                Language.Get(typeof(AmbTokenDetermLang)).TargetParserRuntime);

            Assert.AreEqual(new [] { typeof(Identifier) }, Parse("$ident myvar"));
            Assert.AreEqual(new [] { typeof(Identifier) }, Parse("$ident while"));
            Assert.AreEqual(new [] { typeof(WhileKwd) }, Parse("$keyword while"));
        }

        [Test]
        public void AmbiguousParserTest()
        {
            Assert.AreNotEqual(
                ParserRuntime.Deterministic,
                Language.Get(typeof(AmbTokenAmbLang)).TargetParserRuntime);

            Assert.AreEqual(new [] { typeof(Identifier) }, ParseAmb("myvar $ident"));
            Assert.AreEqual(new [] { typeof(Identifier) }, ParseAmb("while $ident"));
            Assert.AreEqual(new [] { typeof(WhileKwd) }, ParseAmb("while $keyword"));
        }

        private static Type[] Parse(string input)
        {
            var context = new AmbTokenDetermLang();
            return Language.Parse(context, input).ResultTypes.ToArray();
        }

        private static Type[] ParseAmb(string input)
        {
            var context = new AmbTokenAmbLang();
            return Language.Parse(context, input).ResultTypes.ToArray();
        }

        [Language]
        [DescribeParserStateMachine("AmbTokenDetermLang.info")]
        [ScannerGraph("AmbTokenDetermLang_Scanner.gv")]
        public class AmbTokenDetermLang
        {
            public readonly List<Type> ResultTypes = new List<Type>();

            [Produce("$ident")]
            public void Done(Identifier id) { ResultTypes.Add(typeof(Identifier)); }

            [Produce("$keyword")]
            public void Done(WhileKwd kwd) { ResultTypes.Add(typeof(WhileKwd)); }

            [Match("alpha alnum*")]
            public Identifier Ident() { return null; }

            [Literal("while", Disambiguation.Contextual)]
            public WhileKwd WhileKwd() { return null; }

            [Match("blank+")]
            public void WhiteSpace() { }
        }

        [Language(LanguageFlags.AllowNonDeterministic)]
        [DescribeParserStateMachine("AmbTokenAmbLang.info")]
        [ScannerGraph("AmbTokenAmbLang_Scanner.gv")]
        public class AmbTokenAmbLang
        {
            public readonly List<Type> ResultTypes = new List<Type>();

            [Produce(null, "$ident")]
            public void Done(Identifier id) { ResultTypes.Add(typeof(Identifier)); }

            [Produce(null, "$keyword")]
            public void Done(WhileKwd kwd) { ResultTypes.Add(typeof(WhileKwd)); }

            [Match("alpha alnum*")]
            public Identifier Ident() { return null; }

            [Literal("while", Disambiguation.Contextual)]
            public WhileKwd WhileKwd() { return null; }

            [Match("blank+")]
            public void WhiteSpace() { }
        }

        public interface WhileKwd { }
        public interface Identifier { }
    }
}
