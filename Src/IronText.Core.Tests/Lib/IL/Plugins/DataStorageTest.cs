using IronText.Framework;
using IronText.Lib.IL;
using IronText.Lib.IL.Backend.Cecil;
using IronText.Logging;
using IronText.Tests.TestUtils;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace IronText.Tests.Lib.IL.Plugins
{
    [TestFixture]
    public class DataStorageTest
    {
        [Test]
        public void StaticDataFieldUsecase()
        {
            const string AssemblyName = "ass2";
            const string AssemblyFileName = AssemblyName + ".exe";

            byte[] data  = Enumerable.Range(1, 1000000).Select(i => (byte)(i & 0xff)).ToArray();
            byte[] data2 = Enumerable.Range(1, 1000000).Select(i => (byte)((~i) & 0xff)).ToArray();
            
            CilSyntax cilSyntax = CecilBackend.Create(AssemblyFileName);
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

                        .Field()
                            .Public()
                            .Static()
                            .OfType(bytesType)
                            .Named("myBytes2")

                        .Method()
                            .Static.Hidebysig.Rtspecialname.Specialname
                            .Returning(syntax.Types.Void)
                            .Named(".cctor")
                            .BeginArgs().EndArgs()
                        .BeginBody()
                            // Load field #1
                            .With<DataStorage>().Load(data)
                            .Stsfld(
                                new FieldSpec
                                {
                                    FieldType = bytesType,
                                    DeclType  = syntax.Types.Class_(ClassName.Parse("MyClass")),
                                    FieldName = "myBytes"
                                })
                            // Load field #2
                            .With<DataStorage>().Load(data2)
                            .Stsfld(
                                new FieldSpec
                                {
                                    FieldType = bytesType,
                                    DeclType  = syntax.Types.Class_(ClassName.Parse("MyClass")),
                                    FieldName = "myBytes2"
                                })
                            .Ret()
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
            w.Write(AssemblyFileName);
            var outcome = ProgramExecutor.Execute("ass2.exe").Trim();

            Assert.AreEqual(data.Length + "", outcome);

            File.Delete(AssemblyFileName);
        }
    }
}
