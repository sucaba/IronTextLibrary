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

        public virtual IEnumerable<CilSymbolRef> GetTokensInCategory(SymbolCategory category)
        {
            return Enumerable.Empty<CilSymbolRef>();
        }

        public virtual IEnumerable<CilSymbolFeature<Precedence>> GetTokenPrecedence()
        {
            return Enumerable.Empty<CilSymbolFeature<Precedence>>();
        }

        public virtual IEnumerable<CilSymbolFeature<CilContextProvider>> GetTokenContextProvider()
        {
            return Enumerable.Empty<CilSymbolFeature<CilContextProvider>>();
        }

        public virtual IEnumerable<ICilMetadata> GetChildren()
        {
            return Enumerable.Empty<ICilMetadata>();
        }

        public virtual IEnumerable<CilProduction> GetProductions(IEnumerable<CilSymbolRef> leftSides)
        {
            return Enumerable.Empty<CilProduction>();
        }

        public virtual IEnumerable<CilMerger> GetMergeRules(IEnumerable<CilSymbolRef> leftSides)
        {
            return Enumerable.Empty<CilMerger>();
        }

        public virtual IEnumerable<CilScanProduction> GetScanRules()
        {
            return Enumerable.Empty<CilScanProduction>();
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
