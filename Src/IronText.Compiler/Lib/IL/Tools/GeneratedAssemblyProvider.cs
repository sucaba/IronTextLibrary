using System;
using System.IO;
using System.Reflection;
using IronText.Build;
using IronText.Framework;
using IronText.Lib.IL.Backend.Cecil;
using IronText.Logging;

namespace IronText.Lib.IL
{
    public abstract class GeneratedAssemblyProvider 
        : IExternalResourceProvider<Assembly>
    {
        private readonly string assemblyName;
        private Assembly assembly;

        public GeneratedAssemblyProvider(string assemblyName)
        {
            this.assemblyName = assemblyName;
        }

        Assembly IExternalResourceProvider<Assembly>.Resource
        {
            get { return this.assembly; }
        }

        bool IExternalResource.Exists
        {
            get { return false; }
        }

        DateTime IExternalResource.Timestamp
        {
            get { return DateTime.MinValue; }
        }

        System.Collections.ObjectModel.ReadOnlyCollection<IExternalResource> IExternalResource.Sources
        {
            get { return Array.AsReadOnly(new IExternalResource[0]); }
        }

        bool IExternalResource.Rebuild(ILogging logging)
        {
            return DoRebuild(ref this.assembly);
        }

        bool IExternalResource.Load(ILogging logging)
        {
            return true;
        }

        private bool DoRebuild(ref Assembly resource)
        {
            using (var stream = new MemoryStream(4096))
            {
                var backend = new CecilBackend();

                CilSyntax context = backend;

                CilDocumentSyntax docCode = 
                context
                .BeginDocument()

                    .Assembly(assemblyName)
                    .EndAssembly()

                    .AssemblyExtern(context.ResolutionScopeNs.DefineReferencedAssemblyName(new Name1("mscorlib")))
                        .Version(4, 0, 0, 0)
                        .PublicKeyToken(new Bytes(new byte[] { 0xB7, 0x7A, 0x5C, 0x56, 0x19, 0x34, 0xE0, 0x89 }))
                    .EndAssemblyExtern()
                    ;

                docCode = DoGenerate(docCode);
                if (docCode == null)
                {
                    return false;
                }

                docCode.EndDocument();

                var writer = backend as IAssemblyWriter;
                if (writer == null)
                {
                    throw new InvalidOperationException("Backend does not support assembly writing");
                }

#if false
                var path = Path.Combine(Environment.CurrentDirectory, assemblyName + ".dll");
                writer.Write(path);
                assembly = Assembly.LoadFrom(path);
#else
                writer.Write(stream);
                assembly = Assembly.Load(stream.GetBuffer());
#endif
            }

            return true;
        }

        protected abstract CilDocumentSyntax DoGenerate(CilDocumentSyntax docCode);
    }

}
