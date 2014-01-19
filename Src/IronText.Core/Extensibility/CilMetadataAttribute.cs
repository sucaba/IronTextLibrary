using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IronText.Collections;
using IronText.Framework;
using IronText.Misc;

namespace IronText.Extensibility
{
    public abstract class LanguageMetadataAttribute : Attribute, ICilMetadata
    {
        public virtual void Bind(ICilMetadata parent, MemberInfo member)
        {
            this.Parent = parent;
            this.Member = member;
        }

        public virtual bool Validate(ILogging logging) { return true; }

        public ICilMetadata Parent { get; private set; }

        public MemberInfo Member { get; set; }

        public virtual IEnumerable<CilSymbolRef> GetTokensInCategory(ITokenPool tokenPool, SymbolCategory category)
        {
            return Enumerable.Empty<CilSymbolRef>();
        }

        public virtual IEnumerable<TokenFeature<Precedence>> GetTokenPrecedence(ITokenPool tokenPool)
        {
            return Enumerable.Empty<TokenFeature<Precedence>>();
        }

        public virtual IEnumerable<TokenFeature<CilContextProvider>> GetTokenContextProvider(ITokenPool tokenPool)
        {
            return Enumerable.Empty<TokenFeature<CilContextProvider>>();
        }

        public virtual IEnumerable<ICilMetadata> GetChildren()
        {
            return Enumerable.Empty<ICilMetadata>();
        }

        public virtual IEnumerable<CilProductionDef> GetProductions(IEnumerable<CilSymbolRef> leftSides, ITokenPool tokenPool)
        {
            return Enumerable.Empty<CilProductionDef>();
        }

        public virtual IEnumerable<CilMergerDef> GetMergeRules(IEnumerable<CilSymbolRef> leftSides, ITokenPool tokenPool)
        {
            return Enumerable.Empty<CilMergerDef>();
        }

        public virtual IEnumerable<ICilScanRule> GetScanRules(ITokenPool tokenPool)
        {
            return Enumerable.Empty<ICilScanRule>();
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
