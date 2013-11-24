using IronText.Framework;
using IronText.Framework.Reflection;
using NUnit.Framework;

namespace IronText.Tests.Framework
{
    [TestFixture]
    public class RuntimeBnfGrammarTest
    {
        [Test]
        public void TestRightTrimNullable()
        {
            var grammar = new EbnfGrammar();

            var S = grammar.Symbols.Add("S").Index;
            var a = grammar.Symbols.Add("a").Index;
            var b = grammar.Symbols.Add("b").Index;
            var A = grammar.Symbols.Add("A").Index;
            var B = grammar.Symbols.Add("B").Index;

            grammar.StartToken = S;
            grammar.Productions.Add(S, new[] { b, A });
            grammar.Productions.Add(A, new[] { a, A, B });
            grammar.Productions.Add(A, new int[0]);
            grammar.Productions.Add(B, new int[0]);

            var target = new RuntimeEbnfGrammar(grammar);

            Assert.IsTrue(target.IsNullable(A));
            Assert.IsTrue(target.IsNullable(B));

            Assert.IsFalse(target.IsNullable(a));
            Assert.IsFalse(target.IsNullable(b));
            Assert.IsFalse(target.IsNullable(S));
        }
    }
}
