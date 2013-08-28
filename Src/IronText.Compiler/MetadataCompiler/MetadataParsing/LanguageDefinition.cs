using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Extensibility;
using IronText.Framework;

namespace IronText.MetadataCompiler
{
    internal class LanguageDefinition : ITokenPool
    {
        private readonly List<ILanguageMetadata> allMetadata;
        private readonly List<ParseRule>      allParseRules;
        private readonly List<ScanMode>       allScanModes;
        private readonly List<SwitchRule>     allSwitchRules;
        private readonly List<KeyValuePair<TokenRef,Precedence>> precedence;

        private readonly MergeRule[] allMergeRules;

        public LanguageDefinition(Type startType, ILogging logging)
        {
            ITokenPool tokenPool = this;

            var startMeta = MetadataParser.EnumerateAndBind(startType);
            if (startMeta.Count() == 0)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "No metadata found in language definition '{0}'",
                        startType.FullName));
            }

            this.TokenRefResolver = new TokenRefResolver();

            this.allSwitchRules = new List<SwitchRule>();
            this.Start = tokenPool.AugmentedStart;


            var collector = new MetadataCollector(this, logging);
            collector.AddToken(Start);
            foreach (var meta in startMeta)
            {
                collector.AddMeta(meta);
            }

            // collector.AddMeta(new InheritanceMetadata());

            this.allMetadata = collector.AllMetadata;
            this.allParseRules = collector.AllParseRules;

            foreach (var tid in collector.AllTokens)
            {
                TokenRefResolver.Link(tid);
            }

            var categories = new [] { TokenCategory.Beacon, TokenCategory.DoNotInsert, TokenCategory.DoNotDelete, TokenCategory.ExplicitlyUsed };

            foreach (var category in categories)
            {
                foreach (var tokenRef in allMetadata.SelectMany(m => m.GetTokensInCategory(this, category)))
                {
                    var def = TokenRefResolver.Resolve(tokenRef);
                    def.Categories |= category; 
                }
            }

            if (allParseRules.Count == 0)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Language definition '{0}' should have at least one parse rule",
                        startType.FullName));
            }

            allParseRules = allParseRules.Distinct().ToList();

            this.allMergeRules 
                = allMetadata
                    .SelectMany(meta => meta.GetMergeRules(collector.AllTokens, this))
                    .ToArray();

            var terminalsAndExternals = collector.AllTokens.Except(allParseRules.Select(r => r.Left).Distinct()).ToArray();

            var terminals = terminalsAndExternals.Where(
                                token =>
                                {
                                    if (token.IsExternal)
                                    {
                                        var tokens = new [] { token };
                                        var switchRules = allMetadata.SelectMany(meta => meta.GetSwitchRules(tokens, this)).ToArray();
                                        if (switchRules.Length != 0)
                                        {
                                            allSwitchRules.AddRange(switchRules);
                                            return false;
                                        }
                                    }

                                    return true;
                                })
                                // Because predicate has side-effects converting to array avoids 
                                // multiple invocations of predicate.
                                .ToArray();

            var scanDataCollector = new ScanDataCollector(terminals, this, logging);
            scanDataCollector.AddScanMode(startType);
            allScanModes = scanDataCollector.ScanModes;
            LinkRelatedTokens(allScanModes);

            precedence = allMetadata.SelectMany(m => m.GetTokenPrecedence(tokenPool)).ToList();

            ContextTypes = allMetadata.SelectMany(m => m.GetContextTypes()).ToArray();

            this.LanguageDataActions = allMetadata.SelectMany(m => m.GetLanguageDataActions()).ToArray();
        }

        private void LinkRelatedTokens(List<ScanMode> allScanModes)
        {
            foreach (var scanMode in allScanModes)
            {
                foreach (var scanRule in scanMode.ScanRules)
                {
                    foreach (TokenRef[] tokenGroup in scanRule.GetTokenRefGroups())
                    {
                        if (tokenGroup.Any(TokenRefResolver.Contains))
                        {
                            TokenRefResolver.Link(tokenGroup);
                        }
                    }
                }
            }
        }

        public TokenRef Start { get; set; }

        public LanguageDataAction[] LanguageDataActions { get; private set; }

        public TokenRefResolver TokenRefResolver { get; private set; }

        public IEnumerable<KeyValuePair<TokenRef,Precedence>> Precedence { get { return precedence; } }

        public IList<ParseRule> ParseRules { get { return allParseRules; } }

        public IList<MergeRule> MergeRules { get { return allMergeRules; } }

        public IList<ScanMode> ScanModes { get { return allScanModes; } }

        public IList<SwitchRule> SwitchRules { get { return allSwitchRules; } }

        public IList<Type> ContextTypes { get; private set; }

        TokenRef ITokenPool.AugmentedStart
        {
            get { return TokenRef.Typed(typeof(void)); }
        }

        TokenRef ITokenPool.ScanSkipToken
        {
            get { return TokenRef.Typed(typeof(void)); }
        }

        TokenRef ITokenPool.GetToken(Type tokenType)
        {
            return TokenRef.Typed(tokenType);
        }

        TokenRef ITokenPool.GetLiteral(string keyword)
        {
            return TokenRef.Literal(keyword);
        }

        private static IEnumerable<T> EnumerableMutable<T>(IList<T> list)
        {
            for (int i = 0; i != list.Count; ++i)
            {
                yield return list[i];
            }
        }
    }
}
