using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using IronText.Lib.Ctem;
using IronText.Lib.IL;
using IronText.Lib.IL.Backend.Cecil;
using IronText.Logging;

namespace IronText.Build
{
    public class DerivedAssemblyProvider : AssemblyProviderBase
    {
        private readonly Assembly sourceAssembly;
        private readonly ReadOnlyCollection<IExternalResource> _sources;
        private readonly string sourcePath;
        private readonly string derivedPath;

        public DerivedAssemblyProvider(Assembly sourceAssembly, string derivedPath)
            : base(new AssemblyName(sourceAssembly.GetName().Name + ".Derived"))
        {
            this.sourceAssembly = sourceAssembly;
            _sources = new ReadOnlyCollection<IExternalResource>(
                        new [] { new RequiredAssemblyProvider(sourceAssembly.GetName()) });

            if (string.IsNullOrEmpty(derivedPath))
            {
                this.sourcePath = new Uri(sourceAssembly.CodeBase).LocalPath;
                this.derivedPath = Path.ChangeExtension(sourcePath, ".Derived.dll");
            }
            else
            {
                this.derivedPath = derivedPath;
            }

            base.AssemblyName.CodeBase = new Uri(this.derivedPath).AbsoluteUri;
        }

        public override ReadOnlyCollection<IExternalResource> Sources { get { return _sources; } }

        private IDerivedBuilder<CilDocumentSyntax> GetBuilder()
        {
            IMetadataSyntax<CilDocumentSyntax> metadataSyntax = new TypesMetadataSyntax<CilDocumentSyntax>();
            return metadataSyntax.GetBuilder(sourceAssembly);
        }
        
        protected override bool DoRebuild(ILogging logging, ref Assembly resource)
        {
            CilSyntax context = CecilBackend.Create(null);

            var builder = GetBuilder();

            ((IAssemblyResolverParameters)context).AddSearchDirectory(
                Path.GetDirectoryName(derivedPath));

            var cilDocument = context
            .BeginDocument()
                .Assembly(AssemblyName.Name)
                    .CustomAttribute(
                        context.Types.Import(typeof(DerivedAssemblyMarker)))
                .EndAssembly()
                .AssemblyExtern(context.ResolutionScopeNs.DefineReferencedAssemblyName(new Name1("mscorlib")))
                    .Version(4, 0, 0, 0)
                    .PublicKeyToken(new Bytes(new byte[] { 0xB7, 0x7A, 0x5C, 0x56, 0x19, 0x34, 0xE0, 0x89 }))
                .EndAssemblyExtern()
                .Module(new QStr(derivedPath))
                ;

            cilDocument = Build(builder, logging, cilDocument);
            if (cilDocument == null)
            {
                logging.Write(
                    new LogEntry
                    {
                        Severity = Severity.Error,
                        Message = string.Format("Failed to compile assembly '{0}'.", derivedPath)
                    });
                return false;
            }

            cilDocument .EndDocument();

            var writer = context as IAssemblyWriter;
            if (writer == null)
            {
                throw new InvalidOperationException("Backend does not support assembly writing");
            }

            writer.Write(derivedPath);

            logging.Write(
                new LogEntry
                {
                    Severity = Severity.Verbose,
                    Message = string.Format("Saved derived assembly {0}", AssemblyName.Name)
                });

            return true;
        }

        private static CilDocumentSyntax Build(
            IDerivedBuilder<CilDocumentSyntax> builder,
            ILogging logging,
            CilDocumentSyntax context)
        {
            return builder.Build(logging, context);
        }

        public override string ToString()
        {
            return derivedPath;
        }

        public override bool Equals(object obj)
        {
            var casted = obj as DerivedAssemblyProvider;
            return casted != null
                && object.Equals(casted.sourceAssembly, sourceAssembly);
        }

        public override int GetHashCode()
        {
            return ~sourceAssembly.GetHashCode();
        }
    }
}
