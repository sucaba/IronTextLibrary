using System;
using System.Collections.Generic;
using System.IO;
using IronText.Framework;
using Moq;
using NUnit.Framework;

namespace IronText.Tests.Framework
{
    [TestFixture]
    public class GlrTest
    {
        [Test]
        public void SupportsTrvialLanguage()
        {
            Assert.IsTrue(GlrParse<Trivial>(""));
            Assert.IsFalse(GlrParse<Trivial>("a"));
        }

        [Test]
        public void NullableStartLanguage()
        {
            Assert.IsTrue(GlrParse<NullableStart>(""));
            Assert.IsTrue(GlrParse<NullableStart>("a"));
            Assert.IsTrue(GlrParse<NullableStart>("b"));

            Assert.IsFalse(GlrParse<NullableStart>("c"));
            Assert.IsFalse(GlrParse<NullableStart>("ab"));
        }

        [Test]
        public void ParserHandlesNonAmbiguousGrammar()
        {
            Assert.IsTrue(GlrParse<NonAmbiguous>("foobar"));
        }

        [Test]
        public void SupportsSimpleAmbiguousGrammar()
        {
            Assert.IsTrue(GlrParse<SimpleAmbiguousGrammar>("aaaaa"));
        }

        [Test]
        public void SupportsHiddenLeftRecursion()
        {
            Assert.IsTrue(GlrParse<HiddenLeftRecursion>("aa"));
        }

        [Test]
        public void SupportsHiddenRightRecursion()
        {
            Assert.IsTrue(GlrParse<HiddenRightRecursion>("baa"));
        }

        [Test]
        public void RightNullable0Test()
        {
            Assert.IsTrue(GlrParse<RightNullable0>("aa"));
        }

        [Test]
        public void SupportsRightNullableRules()
        {
            Assert.IsTrue(GlrParse<RightNullable>("aaab"));
        }

        [Test]
        public void SupportsNondeterministicReduces()
        {
            Assert.IsTrue(GlrParse<NondeterministicReduce>("b+b+b"));
        }

        [Test]
        public void CanProduceMultipleResults()
        {
            var context = new NondeterministicCalc();
            Assert.IsTrue(GlrParse(context, "3^3^3"));
            Assert.AreEqual(1, context.Results.Count, "Results should be merged");
        }

        [Test]
        public void ParserThrowsExceptionOnError()
        {
            Assert.IsTrue(GlrParse<SimpleAmbiguousGrammar>("aaa"));

            Assert.IsFalse(GlrParse<SimpleAmbiguousGrammar>("aa"));
        }

        private bool GlrParse<T>(string input)
            where T : class
        {
            var mock = new Mock<T>();
            return GlrParse(mock.Object, input);
        }

        private bool GlrParse<T>(T context, string input)
            where T : class
        {
            var lang = Language.Get(typeof(T));
            Assert.IsTrue(lang.IsAmbiguous);

            using (var interpreter = new Interpreter<T>(context) { LogKind = LoggingKind.Collection })
            using (var reader = new StringReader(input))
            {
                return interpreter.Parse(reader, Loc.MemoryString);
            }
        }

        [Language(forceGlr: true)]
        [ParserGraph("NullableStart_Parser.gv")]
        [DescribeParserStateMachine("NullableStart.info")]
        public interface NullableStart
        {
            [Parse]
            void All(A a);
    
            [Parse]
            void All(B s);

            [Parse]
            A Aempty();

            [Parse("a")]
            A A();

            [Parse]
            B Bempty();

            [Parse("b")]
            B B();
        }

        /// <summary>
        /// Simple ambiguous grammar from 4.2
        /// </summary>
        [Language(forceGlr: true)]
        [DescribeParserStateMachine("SimpleAmbiguousGrammar.info")]
        public interface SimpleAmbiguousGrammar
        {
            [Parse("a", null, "a")]
            void All(D s);

            [Parse]
            D D(B b);

            [Parse("a")]
            D D();

            [Parse("a")]
            B B(D d);
        }

        /// <summary>
        /// An example with hidden left recursion
        /// </summary>
        [Language(forceGlr: true)]
        [ParserGraph("HiddenLeftRecursion.gv")]
        [DescribeParserStateMachine("HiddenLeftRecursion.info")]
        public interface HiddenLeftRecursion
        {
            [Parse]
            void All(S s);

            [Parse]
            S S();

            [Parse(null, null, "a")]
            S S(S s1, S s2);
        }

        /// <summary>
        /// An example with hidden right recursion
        /// </summary>
        [Language(forceGlr: true)]
        [DescribeParserStateMachine("LangForTomitaAlogirthm2.info")]
        public interface HiddenRightRecursion
        {
            [Parse("b")]
            void All(A s);

            [Parse]
            A A();

            [Parse("a")]
            A A(A a, B b);

            [Parse]
            B B();
        }

        [Language(forceGlr: true)]
        public interface RightNullable
        {
            [Parse(null, null, "a", "b")]
            void S(B b, D d);

            [Parse("a", null, "a", "d")]
            void S(D d);

            [Parse("a")]
            D D(A a, B b);

            [Parse("a")]
            A A(B b1, B b2);

            [Parse]
            A A();

            [Parse]
            B B();
        }

        /// <summary>
        /// An example with hidden right recursion 
        /// </summary>
        [Language(forceGlr: true)]
        [ParserGraph("RightNullable0.gv")]
        public interface RightNullable0
        {
            [Parse]
            void All(S s);

            [Parse]
            S S();

            [Parse("a")]
            S S(S s, A a);

            [Parse]
            A A();
        }

        [Language(forceGlr: true)]
        public interface NonAmbiguous
        {
            [Parse("foo", "bar")]
            void All();
        }

        [Language(forceGlr: true)]
        [DescribeParserStateMachine("Trivial.info")]
        public interface Trivial
        {
            [Parse]
            void All();
        }

        [Language(forceGlr: true)]
        public interface NondeterministicReduce
        {
            [Parse]
            void All(E e);

            [Parse(null, "+", null)]
            E Add(E e1, E e2);

            [Parse("b")]
            E B();
        }

        [Language(forceGlr: true)]
        public class NondeterministicCalc
        {
            public readonly List<double> Results = new List<double>();

            [Parse]
            public void AddResult(double e) { Results.Add(e); }

            [Parse(null, "^", null)]
            public double Pow(double e1, double e2) { return Math.Pow(e1, e2); }

            [Parse("3")]
            public double Number() { return 3; }
        }


        public interface S {}
        public interface D {}
        public interface A {}
        public interface B {}
        public interface E {}
    }
}
