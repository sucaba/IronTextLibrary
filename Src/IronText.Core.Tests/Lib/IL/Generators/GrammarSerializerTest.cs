using System;
using System.Linq;
using IronText.Framework;
using IronText.Reflection;
using IronText.Lib.IL;
using IronText.MetadataCompiler;
using NUnit.Framework;
using System.Collections.Generic;

namespace IronText.Tests.Lib.IL.Generators
{
    [TestFixture]
    public class GrammarSerializerTest
    {
        [Test]
        public void Test()
        {
            var originalGrammar = new Grammar();
            var red   = originalGrammar.Symbols.Add("red");
            var green = originalGrammar.Symbols.Add("green", SymbolCategory.ExplicitlyUsed);
            var blue  = originalGrammar.Symbols.Add("blue");
            originalGrammar.Productions.Add(red,  new[] { green, blue });
            originalGrammar.Productions.Add(blue, new[] { red, green });
            originalGrammar.Productions.Add(blue, new Symbol[0]);

            originalGrammar.Start = red;

            GrammarSerializer target = new GrammarSerializer(originalGrammar);
            var factory = new CachedMethod<Func<Grammar>>("GrammarSerializerTest.Assembly0", (emit, args) => { target.Build(emit); return emit.Ret(); }).Delegate;

            var recreated = factory();

            Assert.IsTrue(GrammarEquals(originalGrammar, recreated));
        }

        public static bool GrammarEquals(Grammar x, Grammar y)
        {
            return x == y
                || (x != null
                && y != null
                && Enumerable.SequenceEqual(y.Productions, x.Productions, ProductionIndexComparer.Instance)
                && Enumerable.SequenceEqual(y.Symbols, x.Symbols, SymbolIndexComparer.Instance));
        }
    }
}
