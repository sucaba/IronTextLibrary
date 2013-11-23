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

            var S = target.DefineSymbol("S");
            var a = target.DefineSymbol("a");
            var b = target.DefineSymbol("b");
            var A = target.DefineSymbol("A");
            var B = target.DefineSymbol("B");

            target.StartToken = S;
            target.DefineProduction(S, new[] { b, A });
            target.DefineProduction(A, new[] { a, A, B });
            target.DefineProduction(A, new int[0]);
            target.DefineProduction(B, new int[0]);

            target.Freeze();

            Assert.IsTrue(target.IsNullable(A));
            Assert.IsTrue(target.IsNullable(B));

            Assert.IsFalse(target.IsNullable(a));
            Assert.IsFalse(target.IsNullable(b));
            Assert.IsFalse(target.IsNullable(S));

            CollectionAssert.AreEqual(
                new int[0],
                target.TrimRightNullable(new int[0]));
            CollectionAssert.AreEqual(
                new[] { a },
                target.TrimRightNullable(new[] { a }));
            CollectionAssert.AreEqual(
                new int[0],
                target.TrimRightNullable(new[] { A }));
            CollectionAssert.AreEqual(
                new[] { a },
                target.TrimRightNullable(new[] { a, A, B }));

            Assert.AreEqual(0, target.FirstNonNullableCount(new int[0]));
            Assert.AreEqual(1, target.FirstNonNullableCount(new[] { a }));
            Assert.AreEqual(0, target.FirstNonNullableCount(new[] { A }));
            Assert.AreEqual(2, target.FirstNonNullableCount(new[] { a, b, A, B }));
            Assert.AreEqual(3, target.FirstNonNullableCount(new[] { a, A, b, B }));
        }
    }
}
