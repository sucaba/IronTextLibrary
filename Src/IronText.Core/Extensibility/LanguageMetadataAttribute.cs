using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IronText.Framework;
using IronText.Misc;

namespace IronText.Extensibility
{
    public abstract class LanguageMetadataAttribute : Attribute, ILanguageMetadata
    {
        public virtual void Bind(ILanguageMetadata parent, MemberInfo member)
        {
            this.Parent = parent;
            this.Member = member;
        }

        public virtual bool Validate(ILogging logging) { return true; }

        public ILanguageMetadata Parent { get; private set; }

        public MemberInfo Member { get; private set; }

        public virtual IEnumerable<TokenRef> GetTokensInCategory(ITokenPool tokenPool, TokenCategory category)
        {
            return Enumerable.Empty<TokenRef>();
        }

        public virtual IEnumerable<KeyValuePair<TokenRef,Precedence>> GetTokenPrecedence(ITokenPool tokenPool)
        {
            return Enumerable.Empty<KeyValuePair<TokenRef,Precedence>>();
        }

        public virtual IEnumerable<ILanguageMetadata> GetChildren()
        {
            return Enumerable.Empty<ILanguageMetadata>();
        }

        public virtual IEnumerable<ParseRule> GetParseRules(IEnumerable<TokenRef> leftSides, ITokenPool tokenPool)
        {
            return Enumerable.Empty<ParseRule>();
        }

        public virtual IEnumerable<MergeRule> GetMergeRules(IEnumerable<TokenRef> leftSides, ITokenPool tokenPool)
        {
            return Enumerable.Empty<MergeRule>();
        }

        public virtual IEnumerable<SwitchRule> GetSwitchRules(IEnumerable<TokenRef> tokens, ITokenPool tokenPool)
        {
            return Enumerable.Empty<SwitchRule>();
        }

        public virtual IEnumerable<ScanRule> GetScanRules(ITokenPool tokenPool)
        {
            return Enumerable.Empty<ScanRule>();
        }

        public virtual IEnumerable<Type> GetContextTypes()
        {
            return Enumerable.Empty<Type>();
        }

        public virtual IEnumerable<ReportBuilder> GetReportBuilders()
        {
            return Enumerable.Empty<ReportBuilder>();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LanguageMetadataAttribute);
        }

        public bool Equals(LanguageMetadataAttribute other)
        {
            return PropertyComparer<LanguageMetadataAttribute>.Default.Equals(this, other);
        }

        public override int GetHashCode()
        {
            return PropertyComparer<LanguageMetadataAttribute>.Default.GetHashCode(this);
        }

        public override string ToString()
        {
            return string.Format("{{{0} {1}}}", GetType().Name, Member.Name);
        }
    }
}
