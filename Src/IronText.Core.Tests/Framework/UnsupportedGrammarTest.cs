
namespace IronText.Tests.Framework
{
#if false
    [TestFixture]
    public class UnsupportedGrammarTest
    {
        [Test]
        public void Test()
        {
            Language.Parse(new Unsupported(), "xxx");
        }

        public class S { }

        [Language]
        [DescribeParserStateMachine("Unsupported.info")]
        public class Unsupported
        {
            [Parse]
            public void Result(S s) { }

            [Parse("x", null, "x", 
                Precedence =  1, 
                Assoc = Associativity.Left)]
            public S Composite(S inner) { return null; }

            [Parse("x")]
            public S Leaf() { return null; }
        }
    }
#endif
}
