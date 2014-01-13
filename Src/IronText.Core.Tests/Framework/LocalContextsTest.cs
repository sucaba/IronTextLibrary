using IronText.Framework;
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
            [Parse]
            public TestSyntax StartSyntax() { return new TestSyntax(); }
        }

        [Language(LanguageFlags.AllowNonDeterministic)]
        [DescribeParserStateMachine("AmbiguousWithLocalState.info")]
        public class AmbiguousWithLocalScope
        {
            // Choice #1: Reduce
            [Parse]
            public TestSyntax StartSyntax() { return new TestSyntax(); }

            // Choice #2: Shift
            [Parse("foo", "bar")]
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

            [Parse]
            public void Done(TestRef id) { }
        }

        [Vocabulary]
        public class MyScope
        {
            [Parse("foo")]
            public TestRef GetRef() { return new TestRef(); }
        }
    }
}
