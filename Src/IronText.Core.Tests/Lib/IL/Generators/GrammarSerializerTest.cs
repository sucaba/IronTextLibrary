using System;
using System.Linq;
using IronText.Framework;
using IronText.Framework.Reflection;
using IronText.Lib.IL;
using IronText.MetadataCompiler;
using NUnit.Framework;

namespace IronText.Tests.Lib.IL.Generators
{
    [TestFixture]
    public class GrammarSerializerTest
    {
        [Test]
        public void Test()
        {
            var originalGrammar = new EbnfGrammar();
            var red   = originalGrammar.Symbols.Add("red");
            var green = originalGrammar.Symbols.Add("green", SymbolCategory.ExplicitlyUsed);
            var blue  = originalGrammar.Symbols.Add("blue");
            originalGrammar.Productions.Add(red,  new[] { green, blue });
            originalGrammar.Productions.Add(blue, new[] { red, green });
            originalGrammar.Productions.Add(blue, new Symbol[0]);

            originalGrammar.Start = red;

            GrammarSerializer target = new GrammarSerializer(originalGrammar);
            var factory = new CachedMethod<Func<EbnfGrammar>>("GrammarSerializerTest.Assembly0", (emit, args) => { target.Build(emit); return emit.Ret(); }).Delegate;

            var recreated = factory();

            Assert.IsTrue(GrammarEquals(originalGrammar, recreated));
        }

        public static bool GrammarEquals(EbnfGrammar x, EbnfGrammar y)
        {
            return x == y
                || (x != null
                && y != null
                && Enumerable.SequenceEqual(y.Productions, x.Productions)
                && Enumerable.SequenceEqual(y.Symbols, x.Symbols));
        }
    }
}
