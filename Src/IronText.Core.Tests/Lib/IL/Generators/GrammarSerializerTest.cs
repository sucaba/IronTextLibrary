using IronText.Lib.IL;
using IronText.MetadataCompiler;
using IronText.Reflection;
using IronText.Runtime;
using NUnit.Framework;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace IronText.Tests.Lib.IL.Generators
{
    [TestFixture]
    public class GrammarSerializerTest
    {
        [Test]
        public void Test()
        {
            var originalGrammar = BuildSampleGrammar();

            GrammarSerializer target = new GrammarSerializer(originalGrammar);
            var factory = new CachedMethod<Func<byte[]>>("GrammarSerializerTest.Assembly0", (emit, args) => { target.Build(emit); return emit.Ret(); }).Delegate;
            var grammarBytes = factory();

            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream(grammarBytes))
            {
                using (var decompress = new DeflateStream(stream, CompressionMode.Decompress, true))
                {
                    var recreated = (Grammar)formatter.Deserialize(decompress);

                    Assert.IsTrue(GrammarEquals(originalGrammar, recreated));
                }
            }
        }

        [Test]
        public void GrammarIsBinarySerializableTest()
        {
            var originalGrammar = BuildSampleGrammar();
            originalGrammar.BuildIndexes();

            var formatter = new BinaryFormatter();

            byte[] grammarBytes;
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, originalGrammar);
                grammarBytes = stream.ToArray();
            }

            using (var stream = new MemoryStream(grammarBytes))
            {
                var recreated = (Grammar)formatter.Deserialize(stream);

                Assert.IsTrue(GrammarEquals(originalGrammar, recreated));
            }
        }

        public static bool GrammarEquals(Grammar x, Grammar y)
        {
            return x == y
                || (x != null
                && y != null
                && Enumerable.SequenceEqual(y.Productions, x.Productions, ProductionIndexComparer.Instance)
                && Enumerable.SequenceEqual(y.Symbols, x.Symbols, SymbolIndexComparer.Instance));
        }

        private static Grammar BuildSampleGrammar()
        {
            var originalGrammar = new Grammar();
            var red = originalGrammar.Symbols.Add("red");
            var green = originalGrammar.Symbols.Add("green", SymbolCategory.ExplicitlyUsed);
            var blue = originalGrammar.Symbols.Add("blue");
            originalGrammar.Productions.Add(red, new[] { green, blue });
            originalGrammar.Productions.Add(blue, new[] { red, green });
            originalGrammar.Productions.Add(blue, new Symbol[0]);

            originalGrammar.Start = red;
            originalGrammar.BuildIndexes();
            return originalGrammar;
        }
    }
}
