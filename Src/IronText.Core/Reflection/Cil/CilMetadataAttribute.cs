using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IronText.Collections;
using IronText.Framework;
using IronText.Logging;
using IronText.Misc;
using IronText.Reflection;
using IronText.Reporting;

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

        public virtual IEnumerable<CilSymbolRef> GetSymbolsInCategory(SymbolCategory category)
        {
            return Enumerable.Empty<CilSymbolRef>();
        }

        public virtual IEnumerable<CilSymbolFeature<Precedence>> GetSymbolPrecedence()
        {
            return Enumerable.Empty<CilSymbolFeature<Precedence>>();
        }

        public virtual IEnumerable<CilSymbolFeature<CilContextProvider>> GetSymbolContextProviders()
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

        public virtual IEnumerable<CilMerger> GetMergers(IEnumerable<CilSymbolRef> leftSides)
        {
            return Enumerable.Empty<CilMerger>();
        }

        public virtual IEnumerable<CilScanProduction> GetScanProductions()
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
