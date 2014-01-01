using System;
using System.Collections.Generic;
using System.Reflection;
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

        IEnumerable<TokenRef> GetTokensInCategory(ITokenPool tokenPool, SymbolCategory category);

        IEnumerable<KeyValuePair<TokenRef,Precedence>> GetTokenPrecedence(ITokenPool tokenPool);

        IEnumerable<ParseRule> GetParseRules(IEnumerable<TokenRef> leftSides, ITokenPool tokenPool);

        IEnumerable<MergeRule> GetMergeRules(IEnumerable<TokenRef> leftSides, ITokenPool tokenPool);

        IEnumerable<IScanRule> GetScanRules(ITokenPool tokenPool);

        IEnumerable<Type> GetContextTypes();

        IEnumerable<ReportBuilder> GetReportBuilders();
    }
}
