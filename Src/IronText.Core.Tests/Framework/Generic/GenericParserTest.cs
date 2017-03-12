using System.IO;
using IronText.Framework;
using IronText.Logging;
using IronText.Runtime;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System;

namespace IronText.Tests.Framework
{
    [TestFixture]
    public class GenericParserTest
    {
        [Test]
        public void SupportsTrivialLanguage()
        {
            Assert.IsTrue(GlrParse<Trivial>(""));
            Assert.IsFalse(GlrParse<Trivial>("a"));
        }

        [Language(RuntimeOptions.ForceGeneric)]
        [DescribeParserStateMachine(nameof(Trivial) + "0.info")]
        public interface Trivial
        {
            [Produce]
            void All();
        }

        [Test]
        public void ParserHandlesNonAmbiguousGrammar()
        {
            Assert.IsTrue(GlrParse<NonAmbiguous>("foobar"));
        }

        [Language(RuntimeOptions.ForceGeneric)]
        public interface NonAmbiguous
        {
            [Produce("foo", "bar")]
            void All();
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

        [Language(RuntimeOptions.ForceGeneric)]
        [ParserGraph("NullableStart_Parser.gv")]
        [DescribeParserStateMachine("NullableStart.info")]
        public interface NullableStart
        {
            [Produce]
            void All(A a);
    
            [Produce]
            void All(B s);

            [Produce]
            A Aempty();

            [Produce("a")]
            A A();

            [Produce]
            B Bempty();

            [Produce("b")]
            B B();
        }

        [Test]
        public void SupportsSimpleAmbiguousGrammar()
        {
            Assert.IsTrue(GlrParse<SimpleAmbiguousGrammar>("aaaaa"));
        }

        /// <summary>
        /// Simple ambiguous grammar from 4.2
        /// </summary>
        [Language(RuntimeOptions.ForceGeneric)]
        [DescribeParserStateMachine("SimpleAmbiguousGrammar0.info")]
        public interface SimpleAmbiguousGrammar
        {
            [Produce("a", null, "a")]
            void All(D s);

            [Produce]
            D D(B b);

            [Produce("a")]
            D D();

            [Produce("a")]
            B B(D d);
        }

        [Test]
        public void SupportsHiddenLeftRecursion()
        {
            Assert.IsTrue(GlrParse<HiddenLeftRecursion>(""));
            Assert.IsTrue(GlrParse<HiddenLeftRecursion>("aa"));
            Assert.IsTrue(GlrParse<HiddenLeftRecursion>("aaaaaaaa"));
        }

        [Language(RuntimeOptions.ForceGeneric)]
        [ParserGraph("HiddenLeftRecursion0.gv")]
        [DescribeParserStateMachine("HiddenLeftRecursion0.info")]
        public interface HiddenLeftRecursion
        {
            [Produce]
            void All(S s);

            [Produce]
            S S();

            [Produce(null, null, "a")]
            S S(S s1, S s2);
        }

        [Test]
        public void SupportsLeftRecursion()
        {
            Assert.IsTrue(GlrParse<LeftRecursion>(""));
            Assert.IsTrue(GlrParse<LeftRecursion>("a"));
            Assert.IsTrue(GlrParse<LeftRecursion>("aaaaaaa"));
        }

        [Language(RuntimeOptions.ForceGeneric)]
        [ParserGraph("LeftRecursion0.gv")]
        [DescribeParserStateMachine("LeftRecursion0.info")]
        public interface LeftRecursion
        {
            [Produce]
            void All(A s);

            [Produce]
            A S();

            [Produce(null, "a")]
            A S(A before);
        }

        [Test]
        public void SupportsRightRecursion()
        {
            Assert.IsTrue(GlrParse<RightRecursion>(""));
            Assert.IsTrue(GlrParse<RightRecursion>("a"));
            Assert.IsTrue(GlrParse<RightRecursion>("aaaaaaa"));
        }

        [Language(RuntimeOptions.ForceGeneric)]
        [ParserGraph("RightRecursion0.gv")]
        [DescribeParserStateMachine("RightRecursion0.info")]
        public interface RightRecursion
        {
            [Produce]
            void All(A s);

            [Produce]
            A S();

            [Produce("a", null)]
            A S(A after);
        }

        [Test]
        public void SupportsHiddenRightRecursion()
        {
            Assert.IsTrue(GlrParse<HiddenRightRecursion>("b"));
            Assert.IsTrue(GlrParse<HiddenRightRecursion>("ba"));
            Assert.IsTrue(GlrParse<HiddenRightRecursion>("baa"));

            Assert.IsFalse(GlrParse<HiddenRightRecursion>("a"));
            Assert.IsFalse(GlrParse<HiddenRightRecursion>("bab"));
        }

        /// <summary>
        /// An example with hidden right recursion
        /// </summary>
        [Language(RuntimeOptions.ForceGeneric)]
        [DescribeParserStateMachine("LangForTomitaAlogirthm2.info")]
        public interface HiddenRightRecursion
        {
            [Produce("b")]
            void All(A s);

            [Produce]
            A A();

            [Produce("a")]
            A A(A a, B b);

            [Produce]
            B B();
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
            Assert.AreEqual(
                ParserRuntime.Generic,
                lang.TargetParserRuntime);

            using (var interpreter = new Interpreter<T>(context) { LoggingKind = LoggingKind.Collect })
            using (var reader = new StringReader(input))
            {
                return interpreter.Parse(reader, Loc.MemoryString);
            }
        }

        /*

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

        [Language(RuntimeOptions.ForceGeneric)]
        [DescribeParserStateMachine("RightNullable.gram")]
        public interface RightNullable
        {
            [Produce(null, null, "a", "b")]
            void S(B b, D d);

            [Produce("a", null, "a", "d")]
            void S(D d);

            [Produce("a")]
            D D(A a, B b);

            [Produce("a")]
            A A(B b1, B b2);

            [Produce]
            A A();

            [Produce]
            B B();
        }

        /// <summary>
        /// An example with hidden right recursion 
        /// </summary>
        [Language(RuntimeOptions.ForceGeneric)]
        [ParserGraph("RightNullable0.gv")]
        [DescribeParserStateMachine("RightNullable0.gram")]
        public interface RightNullable0
        {
            [Produce]
            void All(S s);

            [Produce]
            S S();

            [Produce("a")]
            S S(S s, A a);

            [Produce]
            A A();
        }

        [Language(RuntimeOptions.ForceGeneric)]
        public interface NondeterministicReduce
        {
            [Produce]
            void All(E e);

            [Produce(null, "+", null)]
            E Add(E e1, E e2);

            [Produce("b")]
            E B();
        }

        [Language(RuntimeOptions.ForceGeneric)]
        [DescribeParserStateMachine("NondeterministicCalc.info")]
        public class NondeterministicCalc
        {
            public readonly List<double> Results = new List<double>();

            [Produce]
            public void AddResult(double e) { Results.Add(e); }

            [Produce(null, "^", null)]
            public double Pow(double e1, double e2) { return Math.Pow(e1, e2); }

            [Produce("3")]
            public double Number() { return 3; }
        }
        */

        public interface S {}
        public interface D {}
        public interface A {}
        public interface B {}
        public interface E {}
    }
}
