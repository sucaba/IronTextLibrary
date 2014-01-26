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

        IEnumerable<CilSymbolFeature<Precedence>> GetTokenPrecedence(ITokenPool tokenPool);

        IEnumerable<CilSymbolFeature<CilContextProvider>> GetTokenContextProvider(ITokenPool tokenPool);

        IEnumerable<CilProduction> GetProductions(IEnumerable<CilSymbolRef> leftSides, ITokenPool tokenPool);

        IEnumerable<CilMerger>     GetMergeRules(IEnumerable<CilSymbolRef> leftSides, ITokenPool tokenPool);

        IEnumerable<CilScanProduction>     GetScanRules(ITokenPool tokenPool);

        IEnumerable<ReportBuilder> GetReportBuilders();
    }
}
