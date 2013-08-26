using IronText.Lib.IL;
using NUnit.Framework;

namespace IronText.Tests.Lib.IL
{
    [TestFixture]
    public class CachedMethodTest
    {
        private delegate string TestDelegate(string arg);

        /// <summary>
        /// Problems:
        /// - Caching assemblies using generic JIB functionality
        /// </summary>
        [Test]
        public void Test()
        {
            const string SampleString = "Hello world 01234567890!";
            var target = new CachedMethod<TestDelegate>(
                "print",
                (emit, args) => emit .Ldarg(args[0]) .Ret()
                );

            Assert.AreEqual(SampleString, target.Delegate(SampleString));
        }
    }
}
