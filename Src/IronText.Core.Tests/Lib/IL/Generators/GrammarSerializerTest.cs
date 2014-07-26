using System;
using System.Linq;
using IronText.Framework;
using IronText.Reflection;
using IronText.Lib.IL;
using IronText.MetadataCompiler;
using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IronText.Lib.IL.Backend.Cecil;
using IronText.Logging;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using IronText.Tests.TestUtils;

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

        private static byte[] data = new byte[] { 65, 65, 65, 65, 65, 65, 65, 65, 65, 65 };

        [Test]
        public void BinarySerialization()
        {
            var originalGrammar = new Grammar();
            var red   = originalGrammar.Symbols.Add("red");
            var green = originalGrammar.Symbols.Add("green", SymbolCategory.ExplicitlyUsed);
            var blue  = originalGrammar.Symbols.Add("blue");
            originalGrammar.Productions.Add(red,  new[] { green, blue });
            originalGrammar.Productions.Add(blue, new[] { red, green });
            originalGrammar.Productions.Add(blue, new Symbol[0]);

            originalGrammar.Start = red;

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

        [Test]
        public void StaticDataFieldUsecase()
        {
            CilSyntax cilSyntax = CecilBackend.Create("ass2.exe");
            cilSyntax.Logging = ExceptionLogging.Instance;
            cilSyntax.Parsing = NullParsing.Instance;

            CilDocumentSyntax syntax = 
                cilSyntax
                .BeginDocument()
                    .Assembly("ass2")
                    .EndAssembly()
                    .AssemblyExtern(cilSyntax.ResolutionScopeNs.DefineReferencedAssemblyName(new Name1("mscorlib")))
                        .Version(4, 0, 0, 0)
                        .PublicKeyToken(new Bytes(new byte[] { 0xB7, 0x7A, 0x5C, 0x56, 0x19, 0x34, 0xE0, 0x89 }))
                    .EndAssemblyExtern()
                    ;

            var bytesType = syntax.Types.Import(typeof(byte[]));

            syntax
                // Sample of byte[] field
                .Class_()
                    .Public
                    .Named("MyClass")

                        .Field()
                            .Public()
                            .Static()
                            .OfType(bytesType)
                            .Named("myBytes")
                        .Method()
                            .Static.Hidebysig.Rtspecialname.Specialname
                            .Returning(syntax.Types.Void)
                            .Named(".cctor")
                            .BeginArgs().EndArgs()
                        .BeginBody()
                            .With<DataStorage>().Load(data)
                            .Stsfld(
                                new FieldSpec
                                {
                                    FieldType = bytesType,
                                    DeclType  = syntax.Types.Class_(ClassName.Parse("MyClass")),
                                    FieldName = "myBytes"
                                })
                        .EndBody()

                        .Method()
                            .Public.Static.Returning(syntax.Types.Void)
                            .Named("Main")
                            .BeginArgs().EndArgs()
                        .BeginBody()
                            .EntryPoint()
                            .Ldsfld(
                                new FieldSpec
                                {
                                    FieldType = bytesType,
                                    DeclType  = syntax.Types.Class_(ClassName.Parse("MyClass")),
                                    FieldName = "myBytes"
                                })
                            .Ldlen()
                            .Box(syntax.Types.Int32)
                            .Call((object o) => o.ToString())
                            .Call((string s) => Console.WriteLine(s))
                            .Ret()
                        .EndBody()
                    .EndClass()
                .EndDocument();

            IAssemblyWriter w = (IAssemblyWriter)syntax;
            w.Write("ass2.exe");
            var outcome = ProgramExecutor.Execute("ass2.exe").Trim();
            Assert.AreEqual("10", outcome);
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
