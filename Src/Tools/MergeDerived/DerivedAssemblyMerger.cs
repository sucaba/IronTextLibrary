using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using IronText.Build;
using IronText.Framework;
using IronText.Lib.IL;
using IronText.Misc;

namespace NDerive
{
    public class DerivedAssemblyMerger : MarshalByRefObject
    {
        public void Merge(FileInfo source, FileInfo destination)
        {
            ILogging logging = new BuildLogging();

            var sourceAssemblyName = AssemblyName.GetAssemblyName(source.FullName);
            var coreAssemblyName = typeof(Language).Assembly.GetName();

            Assembly sourceAssembly;

            if (coreAssemblyName.FullName == sourceAssemblyName.FullName)
            {
                sourceAssembly = typeof(Language).Assembly;
            }
            else
            {
                sourceAssembly = Assembly.LoadFrom(source.FullName);
            }

            IMetadataSyntax<CilDocumentSyntax> metadataSyntax = new TypesMetadataSyntax<CilDocumentSyntax>();
            var builder = metadataSyntax.GetBuilder(sourceAssembly);

            Pipe<CilDocumentSyntax> compile = ctx => Build(builder, logging, ctx);

            Merge(source, compile, destination);
        }

        private static CilDocumentSyntax Build(
            IDerivedBuilder<CilDocumentSyntax> builder,
            ILogging logging,
            CilDocumentSyntax context)
        {
            return builder.Build(logging, context);
        }

        public void Merge(
            FileInfo           source,
            Pipe<CilDocumentSyntax> compile,
            FileInfo           destination)
        {
            CilSyntax context = CilLanguage.CreateCompiler(null);
            context
            .BeginDocument()
                .AssemblyRewrite(source.FullName)
                    .CustomAttribute(
                        context.Types.Import(
                            typeof(DerivedAssemblyMarker)))
                .EndAssembly()
                .Do(compile)
            .EndDocument();

            ((IAssemblyResolverParameters)context).AddSearchDirectory(destination.DirectoryName);

            var writer = context as IAssemblyWriter;
            if (writer == null)
            {
                throw new InvalidOperationException("Backend does not support assembly writing");
            }

            writer.Write(destination.FullName);
        }
        
        private static IEnumerable<IDerivedBuilder<CilDocumentSyntax>> GetBuilders(Assembly sourceAssembly)
        {
            foreach (var type in sourceAssembly.GetTypes())
            {
                foreach (IDerivedBuilderMetadata attr in type.GetCustomAttributes(typeof(IDerivedBuilderMetadata), false))
                {
                    if (attr.IsIncludedInBuild(type))
                    {
                        yield return CreateBuilder(attr.BuilderType, type);
                    }
                }
            }
        }

        private static IDerivedBuilder<CilDocumentSyntax> CreateBuilder(Type type, Type attributeDefiningType)
        {
            return (IDerivedBuilder<CilDocumentSyntax>)Activator.CreateInstance(type, attributeDefiningType);
        }
    }
}
