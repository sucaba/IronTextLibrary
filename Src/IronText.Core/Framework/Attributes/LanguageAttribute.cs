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
        , ILanguageMetadata
    {
        private Type type;
        private ILanguageMetadata parent;

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

        ILanguageMetadata ILanguageMetadata.Parent { get { return parent; } }

        MemberInfo ILanguageMetadata.Member { get { return type; } }

        void ILanguageMetadata.Bind(ILanguageMetadata parent, MemberInfo member)
        {
            this.parent = parent;
            this.type = (Type)member;
        }

        IEnumerable<ILanguageMetadata> ILanguageMetadata.GetChildren()
        {
            var result = EnumerateDirectChildren()
                        .Concat(MetadataParser.GetTypeMetaChildren(parent, type))
                        .ToArray();

            return result;
        }

        public IEnumerable<TokenRef> GetTokensInCategory(ITokenPool tokenPool, SymbolCategory category)
        {
            return Enumerable.Empty<TokenRef>();
        }

        public IEnumerable<TokenFeature<Precedence>> GetTokenPrecedence(ITokenPool tokenPool)
        {
            return Enumerable.Empty<TokenFeature<Precedence>>();
        }

        public IEnumerable<TokenFeature<CilContextProvider>> GetTokenContextProvider(ITokenPool tokenPool)
        {
            return Enumerable.Empty<TokenFeature<CilContextProvider>>();
        }

        private IEnumerable<ILanguageMetadata> EnumerateDirectChildren()
        {
            var result = MetadataParser
                .EnumerateAndBind(type)
                .Where(m => !(m is LanguageAttribute))
                .ToArray();

            return result;
        }

        IEnumerable<ParseRule> ILanguageMetadata.GetParseRules(IEnumerable<TokenRef> leftSides, ITokenPool tokenPool)
        {
            return Enumerable.Empty<ParseRule>();
        }

        IEnumerable<MergeRule> ILanguageMetadata.GetMergeRules(IEnumerable<TokenRef> leftSides, ITokenPool tokenPool)
        {
            return Enumerable.Empty<MergeRule>();
        }

        IEnumerable<IScanRule> ILanguageMetadata.GetScanRules(ITokenPool tokenPool)
        {
            return Enumerable.Empty<IScanRule>();
        }

        public virtual IEnumerable<ReportBuilder> GetReportBuilders()
        {
            return Enumerable.Empty<ReportBuilder>();
        }
    }
}
