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
        public void StaticDataFieldUsecase()
        {
            CilSyntax cilSyntax = CecilBackend.Create("ass2.exe");
            cilSyntax.Logging = ExceptionLogging.Instance;
            cilSyntax.Parsing = NullParsing.Instance;
            string implDetailsTypeName = "ImplementationDetails";
            string dataTypeName = "Arraytype" + data.Length;

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

                // Data type
            syntax
                .Class_()
                        .Explicit.Ansi.Sealed
                        .Named(dataTypeName)
                        .Extends(syntax.Types.ValueType)
                    .Pack(1)
                    .Size(10)
                .EndClass() 
                // Implementation details static class
                .Class_()
                        .Public
                        .Named(implDetailsTypeName)
                    .Field()
                        .Private()
                        .Static()
                        .Assembly()
                        .OfType(syntax.Types.Value(ClassName.Parse(dataTypeName)))
                        .Named("dataField")
                        .HasRVA
                        .Init(new Bytes(data))
                .EndClass()
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
                            .Ldc_I4(data.Length)
                            .Newarr(syntax.Types.UnsignedInt8)
                            .Dup()
                            .Ldtoken(
                                new FieldSpec
                                { 
                                    FieldType = syntax.Types.Value(ClassName.Parse(dataTypeName)),
                                    DeclType  = syntax.Types.Class_(ClassName.Parse(implDetailsTypeName)),
                                    FieldName = "dataField"
                                })
                            .Call((Array arr, RuntimeFieldHandle handle) =>
                                RuntimeHelpers.InitializeArray(arr, handle)
                            )
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
