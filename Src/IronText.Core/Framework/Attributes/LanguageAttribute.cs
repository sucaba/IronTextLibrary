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
        internal bool ForceGlr;

        public LanguageAttribute() { }

        internal LanguageAttribute(bool forceGlr) { this.ForceGlr = forceGlr; }

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

        public IEnumerable<TokenRef> GetTokensInCategory(ITokenPool tokenPool, TokenCategory category)
        {
            return Enumerable.Empty<TokenRef>();
        }

        public IEnumerable<KeyValuePair<TokenRef,Precedence>> GetTokenPrecedence(ITokenPool tokenPool)
        {
            return Enumerable.Empty<KeyValuePair<TokenRef,Precedence>>();
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

        IEnumerable<ScanRule> ILanguageMetadata.GetScanRules(ITokenPool tokenPool)
        {
            return Enumerable.Empty<ScanRule>();
        }

        public IEnumerable<Type> GetContextTypes()
        {
            yield break;
        }

        public IEnumerable<SwitchRule> GetSwitchRules(IEnumerable<TokenRef> token, ITokenPool tokenPool)
        {
            return Enumerable.Empty<SwitchRule>();
        }

        public virtual IEnumerable<LanguageDataAction> GetLanguageDataActions()
        {
            return Enumerable.Empty<LanguageDataAction>();
        }
    }
}
