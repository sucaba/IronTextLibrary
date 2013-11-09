using System;
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
            int red = originalGrammar.DefineToken("red");
            int green = originalGrammar.DefineToken("green", TokenCategory.External);
            int blue = originalGrammar.DefineToken("blue");
            originalGrammar.DefineRule(red, new[]{ green, blue });
            originalGrammar.DefineRule(blue, new []{ red, green });
            originalGrammar.DefineRule(blue, new int[0]);

            originalGrammar.Freeze();

            GrammarSerializer target = new GrammarSerializer(originalGrammar);
            var factory = new CachedMethod<Func<EbnfGrammar>>("GrammarSerializerTest.Assembly0", (emit, args) => { target.Build(emit); return emit.Ret(); }).Delegate;

            var recreated = factory();

            Assert.AreEqual(originalGrammar, recreated);
        }
    }
}
