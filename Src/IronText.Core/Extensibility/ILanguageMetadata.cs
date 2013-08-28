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

        IEnumerable<TokenRef> GetTokensInCategory(ITokenPool tokenPool, TokenCategory category);

        IEnumerable<KeyValuePair<TokenRef,Precedence>> GetTokenPrecedence(ITokenPool tokenPool);

        IEnumerable<ParseRule> GetParseRules(IEnumerable<TokenRef> leftSides, ITokenPool tokenPool);

        IEnumerable<MergeRule> GetMergeRules(IEnumerable<TokenRef> leftSides, ITokenPool tokenPool);

        IEnumerable<SwitchRule> GetSwitchRules(IEnumerable<TokenRef> leftSides, ITokenPool tokenPool);

        IEnumerable<ScanRule> GetScanRules(ITokenPool tokenPool);

        IEnumerable<Type> GetContextTypes();

        IEnumerable<ReportBuilder> GetLanguageDataActions();
    }
}
