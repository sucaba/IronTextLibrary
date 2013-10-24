using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework;
using NUnit.Framework;

namespace IronText.Tests.Framework
{
    [TestFixture]
    public class AmbTokenTest
    {
        [Test]
        public void Test()
        {
            Assert.AreEqual(new [] { typeof(Ident) }, Parse("myvar"));
            Assert.AreEqual(new [] { typeof(Ident), typeof(WhileKwd) }, Parse("while"));
        }

        private static Type[] Parse(string input)
        {
            var context = new AmbTokenLang();
            return Language.Parse(context, input).ResultTypes.ToArray();
        }

        [Language]
        [DescribeParserStateMachine("AmbTokenLang.info")]
        [ScannerGraph("AmbTokenLang_Scanner.gv")]
        public class AmbTokenLang
        {
            public readonly List<Type> ResultTypes = new List<Type>();

            [Parse]
            public void Done(Ident id) { ResultTypes.Add(typeof(Ident)); }

            [Parse]
            public void Done(WhileKwd kwd) { ResultTypes.Add(typeof(WhileKwd)); }

            [Scan("alpha alnum*")]
            public Ident Ident() { return null; }

            [Literal("while", Disambiguation.Contextual)]
            public WhileKwd WhileKwd() { return null; }
        }

        public interface WhileKwd { }
        public interface Ident { }
    }
}
