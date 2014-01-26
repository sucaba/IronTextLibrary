using IronText.Framework;
using IronText.Runtime;
using NUnit.Framework;

namespace IronText.Tests.Framework
{
    [TestFixture]
    public class ParseRuleMultiMethodsTest
    {
        [Test]
        public void Test()
        {
            var context = new MyLanguage();
            Language.Parse(context, "foo");
            Assert.AreEqual(1, context.Scope.SingleLiterArgInvocationCount);
            Assert.AreEqual(1, context.SingleLiterArgInvocationCount);

            Assert.AreEqual(
                "foo-global", context.Result,
                "Global rule-method should be called last and should provide rule result-value");
        }

        [Language]
        [GrammarDocument("MyLanguage.gram")]
        [ScannerDocument("MyLanguage.scan")]
        public class MyLanguage
        {
            public int SingleLiterArgInvocationCount = 0;

            public MyLanguage()
            {
                Scope = new MyScope();
            }

            [SubContext]
            public MyScope Scope { get; private set; }

            public string Result { get; [Parse] set; }

            [Parse("foo")]
            public string SingleLiteralArg() 
            { 
                ++SingleLiterArgInvocationCount;
                return "foo-global"; 
            }
        }

        [Vocabulary]
        public class MyScope
        {
            public int SingleLiterArgInvocationCount = 0;

            [Parse("foo")]
            public string SingleLiteralArg() 
            {
                ++SingleLiterArgInvocationCount;
                return "foo-context"; 
            }
        }
    }
}
