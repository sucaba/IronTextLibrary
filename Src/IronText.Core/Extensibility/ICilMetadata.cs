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

        MemberInfo Member { get; }

        void Bind(ICilMetadata parent, MemberInfo member);

        IEnumerable<CilSymbolRef>      GetTokensInCategory(ITokenPool tokenPool, SymbolCategory category);

        IEnumerable<SymbolFeature<Precedence>> GetTokenPrecedence(ITokenPool tokenPool);

        IEnumerable<SymbolFeature<CilContextProvider>> GetTokenContextProvider(ITokenPool tokenPool);

        IEnumerable<CilProductionDef> GetProductions(IEnumerable<CilSymbolRef> leftSides, ITokenPool tokenPool);

        IEnumerable<CilMergerDef>     GetMergeRules(IEnumerable<CilSymbolRef> leftSides, ITokenPool tokenPool);

        IEnumerable<ICilScanRule>     GetScanRules(ITokenPool tokenPool);

        IEnumerable<ReportBuilder> GetReportBuilders();
    }
}
