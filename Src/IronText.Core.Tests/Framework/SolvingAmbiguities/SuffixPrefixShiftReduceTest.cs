using IronText.Framework;
using NUnit.Framework;

namespace IronText.Tests.Framework.SolvingAmbiguities
{
    [TestFixture]
    public class SuffixPrefixShiftReduceTest
    {
        [Test]
        public void Test()
        {
            var context = new SuffixPrefixConflictLang();
            Language.Parse(context, "aaa [ bprefix ] bbb");
            Language.Parse(context, "aaa [ asuffix ] [ bprefix ] bbb");
        }

        [Language]
        [GrammarDocument("SuffixPrefixConflictLang.gram")]
        [ScannerDocument("SuffixPrefixConflictLang.scan")]
        [DescribeParserStateMachine("SuffixPrefixConflictLang.info")]
        public class SuffixPrefixConflictLang
        {
#if false
            [Parse]
            public WantB WantB(A a) { return null; }

            [Parse]
            public void SatisfyWantB(WantB wantb, B b) { }
#else
            [Parse]
            public void Result(A a, B b) {}
#endif

            [Parse("aaa")]
            public A A() { return null; }

#if false
            [Parse("aaa", "[", "asuffix", "]")]
            public A AWithSuffix() { return null; }
#else
            [Parse(null, "[", "asuffix", "]")]
            public A A(A a) { return null; }
#endif

            [Parse("[", "bprefix", "]", "bbb")]
            public B BWithPrefix() { return null; }

            [Scan("blank+")]
            public void Blank() {}
        }

        public interface A {}
        public interface WantB {}
        public interface B {}
        public interface B2 {}
    }
}
