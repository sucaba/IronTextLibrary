using IronText.Framework;
using IronText.MetadataCompiler;
using IronText.Reflection;
using IronText.Runtime;
using NUnit.Framework;

namespace IronText.Tests.Framework
{
    [TestFixture]
    public class RuntimeGrammarTest
    {
        [Test]
        public void IsNullableTest()
        {
            var grammar = new Grammar();

            var S = grammar.Symbols.Add("S");
            var a = grammar.Symbols.Add("a");
            var b = grammar.Symbols.Add("b");
            var A = grammar.Symbols.Add("A");
            var B = grammar.Symbols.Add("B");

            grammar.Start = S;
            grammar.Productions.Add(S, new[] { b, A });
            grammar.Productions.Add(A, new[] { a, A, B });
            grammar.Productions.Add(A, new Symbol[0]);
            grammar.Productions.Add(B, new Symbol[0]);

            grammar.BuildIndexes();

            var provider = new RuntimeGrammarProvider(grammar, null, null, new NullableFirstTables(grammar));
            var target = provider.Outcome;

            Assert.IsTrue(target.IsNullable(A.Index));
            Assert.IsTrue(target.IsNullable(B.Index));

            Assert.IsFalse(target.IsNullable(a.Index));
            Assert.IsFalse(target.IsNullable(b.Index));
            Assert.IsFalse(target.IsNullable(S.Index));
        }
    }
}
