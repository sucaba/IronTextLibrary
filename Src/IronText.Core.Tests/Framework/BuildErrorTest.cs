# if false
using NUnit.Framework;
using IronText.Framework;
using IronText.Runtime;
namespace IronText.Tests.Framework
{
    [TestFixture]
    public class BuildErrorTest
    {
        [Test]
        public void TestDeterminisic()
        {
            Language.Get(typeof(BuildErrorsLang));
        }

        [Test]
        public void TestNonDeterminisic()
        {
            Language.Get(typeof(NonDeterministicBuildErrorsLang));
        }

        [Language]
        public interface BuildErrorsLang
        {
            [Produce]
            void All(string s);

            [Produce]
            void All(int x);

            [Produce]
            int BadTemplate<T>(T token);

            [Literal("foo")]
            string Foo();

            [Match("'x' *")]
            string NullableScanPattern();

            [Literal("")]
            string NullableLiteral();
        }

        [Language]
        public interface NonDeterministicBuildErrorsLang
        {
            [Produce("foo")]
            [Produce("foo")]
            void All();
        }
    }
}
#endif
