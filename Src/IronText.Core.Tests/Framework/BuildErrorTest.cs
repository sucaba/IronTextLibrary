﻿# if false
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
            Language.Get(typeof(NonDeterministicBuildErrorsLang));
        }

        [Language]
        public interface BuildErrorsLang
        {
            [Parse]
            void All(string s);

            [Parse]
            void All(int x);

            [Parse]
            int BadTemplate<T>(T token);

            [Literal("foo")]
            string Foo();

            [Scan("'x' *")]
            string NullableScanPattern();

            [Literal("")]
            string NullableLiteral();
        }

        [Language]
        public interface NonDeterministicBuildErrorsLang
        {
            [Parse("foo")]
            [Parse("foo")]
            void All();
        }
    }
}
#endif
