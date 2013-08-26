#if false
using NUnit.Framework;
using IronText.Framework;
namespace IronText.Tests.Framework
{
    [TestFixture]
    public class BuildErrorTest
    {
        [Test]
        public void TestParse()
        {
            Language.Get(typeof(BuildErrorsLang));
        }

        [Language]
        public interface BuildErrorsLang
        {
            [Parse]
            void All(string s);

            [Scan("'x' *")]
            string NullableScanPattern();

            [Literal("")]
            string NullableLiteral();
        }
    }
}
#endif
