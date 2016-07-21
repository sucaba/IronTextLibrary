using IronText.Framework;
using IronText.Runtime;
using NUnit.Framework;

namespace IronText.Tests.Framework
{
    [TestFixture]
    public class LocalContextsTest
    {
        [Test]
        public void UnambigousGrammarTest()
        {
            var context = new WithLocalScope();
            Language.Parse(context, "foo");
        }

        [Test]
        public void AmbigousGrammarTest()
        {
            var context = new AmbiguousWithLocalScope();
            Language.Parse(context, "foo");
        }

        public class TestRef { }

        [Language]
        [DescribeParserStateMachine("WithLocalState.info")]
        public class WithLocalScope
        {
            [Produce]
            public TestSyntax StartSyntax() { return new TestSyntax(); }
        }

        [Language(RuntimeOptions.AllowNonDeterministic)]
        [DescribeParserStateMachine("AmbiguousWithLocalState.info")]
        public class AmbiguousWithLocalScope
        {
            // Choice #1: Reduce
            [Produce]
            public TestSyntax StartSyntax() { return new TestSyntax(); }

            // Choice #2: Shift
            [Produce("foo", "bar")]
            public TestSyntax FooBar() { return new TestSyntax(); }
        }

        [Demand]
        public class TestSyntax
        {
            /// <summary>
            /// This context can be resolved when TestSyntax is in stack
            /// </summary>
            [SubContext]
            public MyScope Scope { get { return new MyScope(); } }

            [Produce]
            public void Done(TestRef id) { }
        }

        [Vocabulary]
        public class MyScope
        {
            [Produce("foo")]
            public TestRef GetRef() { return new TestRef(); }
        }
    }
}
