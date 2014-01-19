using System;
using System.Collections.Generic;
using System.Reflection;
using IronText.Extensibility.Cil;
using IronText.Framework;

namespace IronText.Extensibility
{
    public interface ILanguageMetadata
    {
        bool Validate(ILogging logging);

        IEnumerable<ILanguageMetadata> GetChildren();

        ILanguageMetadata Parent { get; }

        MemberInfo Member { get; }

        void Bind(ILanguageMetadata parent, MemberInfo member);

        IEnumerable<TokenRef>      GetTokensInCategory(ITokenPool tokenPool, SymbolCategory category);

        IEnumerable<TokenFeature<Precedence>> GetTokenPrecedence(ITokenPool tokenPool);

        IEnumerable<TokenFeature<CilContextProvider>> GetTokenContextProvider(ITokenPool tokenPool);

        IEnumerable<CilProductionDef> GetProductions(IEnumerable<TokenRef> leftSides, ITokenPool tokenPool);

        IEnumerable<CilMergerDef>     GetMergeRules(IEnumerable<TokenRef> leftSides, ITokenPool tokenPool);

        IEnumerable<IScanRule>     GetScanRules(ITokenPool tokenPool);

        IEnumerable<ReportBuilder> GetReportBuilders();
    }
}
