using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IronText.Build;
using IronText.Extensibility;
using IronText.Logging;
using IronText.Reflection;
using IronText.Reflection.Managed;
using IronText.Reflection.Reporting;

namespace IronText.Framework
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Interface, AllowMultiple = false)]
    public class LanguageAttribute 
        : Attribute
        , IDerivedBuilderMetadata
        , ICilMetadata
    {
        private Type type;
        private ICilMetadata parent;

        public LanguageAttribute() { }

        public LanguageAttribute(LanguageFlags flags) 
        {
            this.Flags = flags;
        }

        public LanguageFlags Flags { get; set; }

        public virtual bool Validate(ILogging logging) { return true; }

        Type IDerivedBuilderMetadata.BuilderType 
        { 
            get 
            {
                const string DefaultLanguageBuilderTypeName = "IronText.MetadataCompiler.LanguageDerivedBuilder, IronText.Compiler";
                var result = Type.GetType(DefaultLanguageBuilderTypeName);
                if (result == null)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "Unable to load builder type '{0}'.",
                            DefaultLanguageBuilderTypeName));
                }

                return result;
            }
        }

        bool IDerivedBuilderMetadata.IsIncludedInBuild(Type declaringType) { return true; }

        ICilMetadata ICilMetadata.Parent { get { return parent; } }

        MemberInfo ICilMetadata.Member { get { return type; } }

        void ICilMetadata.Bind(ICilMetadata parent, MemberInfo member)
        {
            this.parent = parent;
            this.type = (Type)member;
        }

        IEnumerable<ICilMetadata> ICilMetadata.GetChildren()
        {
            var result = EnumerateDirectChildren()
                        .Concat(MetadataParser.GetTypeMetaChildren(parent, type))
                        .ToArray();

            return result;
        }

        public IEnumerable<CilSymbolRef> GetSymbolsInCategory(SymbolCategory category)
        {
            return Enumerable.Empty<CilSymbolRef>();
        }

        public IEnumerable<CilSymbolFeature<Precedence>> GetSymbolPrecedence()
        {
            return Enumerable.Empty<CilSymbolFeature<Precedence>>();
        }

        public IEnumerable<CilSymbolFeature<CilContextProvider>> GetSymbolContextProviders()
        {
            return Enumerable.Empty<CilSymbolFeature<CilContextProvider>>();
        }

        private IEnumerable<ICilMetadata> EnumerateDirectChildren()
        {
            var result = MetadataParser
                .EnumerateAndBind(type)
                .Where(m => !(m is LanguageAttribute))
                .ToArray();

            return result;
        }

        IEnumerable<CilProduction> ICilMetadata.GetProductions(IEnumerable<CilSymbolRef> leftSides)
        {
            return Enumerable.Empty<CilProduction>();
        }

        IEnumerable<CilMerger> ICilMetadata.GetMergers(IEnumerable<CilSymbolRef> leftSides)
        {
            return Enumerable.Empty<CilMerger>();
        }

        IEnumerable<CilMatcher> ICilMetadata.GetMatchers()
        {
            return Enumerable.Empty<CilMatcher>();
        }

        public virtual IEnumerable<IReport> GetReports()
        {
            return Enumerable.Empty<IReport>();
        }
    }
}
