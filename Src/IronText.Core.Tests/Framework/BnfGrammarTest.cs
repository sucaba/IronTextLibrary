using IronText.Framework;
using IronText.Framework.Reflection;
using NUnit.Framework;

namespace IronText.Tests.Framework
{
    [TestFixture]
    public class BnfGrammarTest
    {
        [Test]
        public void TestRightTrimNullable()
        {
            var target = new EbnfGrammar();

            var S = target.Symbols.Add("S").Index;
            var a = target.Symbols.Add("a").Index;
            var b = target.Symbols.Add("b").Index;
            var A = target.Symbols.Add("A").Index;
            var B = target.Symbols.Add("B").Index;

            target.StartToken = S;
            target.Productions.Add(S, new[] { b, A });
            target.Productions.Add(A, new[] { a, A, B });
            target.Productions.Add(A, new int[0]);
            target.Productions.Add(B, new int[0]);

            target.Freeze();

            Assert.IsTrue(target.IsNullable(A));
            Assert.IsTrue(target.IsNullable(B));

            Assert.IsFalse(target.IsNullable(a));
            Assert.IsFalse(target.IsNullable(b));
            Assert.IsFalse(target.IsNullable(S));
        }
    }
}
