using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IronText.Build;
using IronText.Extensibility;

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

        public IEnumerable<CilSymbolRef> GetTokensInCategory(ITokenPool tokenPool, SymbolCategory category)
        {
            return Enumerable.Empty<CilSymbolRef>();
        }

        public IEnumerable<SymbolFeature<Precedence>> GetTokenPrecedence(ITokenPool tokenPool)
        {
            return Enumerable.Empty<SymbolFeature<Precedence>>();
        }

        public IEnumerable<SymbolFeature<CilContextProvider>> GetTokenContextProvider(ITokenPool tokenPool)
        {
            return Enumerable.Empty<SymbolFeature<CilContextProvider>>();
        }

        private IEnumerable<ICilMetadata> EnumerateDirectChildren()
        {
            var result = MetadataParser
                .EnumerateAndBind(type)
                .Where(m => !(m is LanguageAttribute))
                .ToArray();

            return result;
        }

        IEnumerable<CilProductionDef> ICilMetadata.GetProductions(IEnumerable<CilSymbolRef> leftSides, ITokenPool tokenPool)
        {
            return Enumerable.Empty<CilProductionDef>();
        }

        IEnumerable<CilMergerDef> ICilMetadata.GetMergeRules(IEnumerable<CilSymbolRef> leftSides, ITokenPool tokenPool)
        {
            return Enumerable.Empty<CilMergerDef>();
        }

        IEnumerable<ICilScanRule> ICilMetadata.GetScanRules(ITokenPool tokenPool)
        {
            return Enumerable.Empty<ICilScanRule>();
        }

        public virtual IEnumerable<ReportBuilder> GetReportBuilders()
        {
            return Enumerable.Empty<ReportBuilder>();
        }
    }
}
