using System;
using System.Collections.Generic;
using System.Reflection;
using IronText.Framework;

namespace IronText.Extensibility
{
    public interface ICilMetadata
    {
        bool Validate(ILogging logging);

        IEnumerable<ICilMetadata> GetChildren();

        ICilMetadata Parent { get; }

        MemberInfo   Member { get; }

        void Bind(ICilMetadata parent, MemberInfo member);

        IEnumerable<CilSymbolRef> GetTokensInCategory(SymbolCategory category);

        IEnumerable<CilSymbolFeature<Precedence>> GetTokenPrecedence();

        IEnumerable<CilSymbolFeature<CilContextProvider>> GetTokenContextProvider();

        IEnumerable<CilProduction> GetProductions(IEnumerable<CilSymbolRef> leftSides);

        IEnumerable<CilMerger> GetMergeRules(IEnumerable<CilSymbolRef> leftSides);

        IEnumerable<CilScanProduction> GetScanRules();

        IEnumerable<ReportBuilder> GetReportBuilders();
    }
}
