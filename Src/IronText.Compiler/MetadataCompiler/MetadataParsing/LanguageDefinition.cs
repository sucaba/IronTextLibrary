using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Extensibility;
using IronText.Framework;

namespace IronText.MetadataCompiler
{
    internal class LanguageDefinition : ITokenPool
    {
        private readonly List<ILanguageMetadata> allMetadata;
        private readonly List<ParseRule>      allParseRules;
        private readonly List<ScanMode>       allScanModes;
        private readonly List<TokenFeature<Precedence>> precedence;

        private readonly MergeRule[] allMergeRules;

        public LanguageDefinition(Type startType, ILogging logging)
        {
            this.IsValid = true;

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

            this.Start = tokenPool.AugmentedStart;


            var collector = new MetadataCollector(this, logging);
            collector.AddToken(Start);
            foreach (var meta in startMeta)
            {
                collector.AddMeta(meta);
            }

            if (collector.HasInvalidData)
            {
                this.IsValid = false;
            }

            // collector.AddMeta(new InheritanceMetadata());

            this.allMetadata = collector.AllMetadata;
            this.allParseRules = collector.AllParseRules;

            foreach (var tid in collector.AllTokens)
            {
                TokenRefResolver.Link(tid);
            }

            var categories = new [] { SymbolCategory.Beacon, SymbolCategory.DoNotInsert, SymbolCategory.DoNotDelete, SymbolCategory.ExplicitlyUsed };

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

            this.allMergeRules 
                = allMetadata
                    .SelectMany(meta => meta.GetMergeRules(collector.AllTokens, this))
                    .ToArray();

            var terminals = collector.AllTokens.Except(allParseRules.Select(r => r.Left).Distinct()).ToArray();

            var scanDataCollector = new ScanDataCollector(terminals, this, logging);
            scanDataCollector.AddScanMode(startType);
            if (scanDataCollector.HasInvalidData)
            {
                this.IsValid = false;
            }

            CheckAllScanRulesDefined(scanDataCollector.UndefinedTerminals, startType, logging);

            allScanModes = scanDataCollector.ScanModes;
            LinkRelatedTokens(allScanModes);

            precedence          = allMetadata.SelectMany(m => m.GetTokenPrecedence(tokenPool)).ToList();

            ContextProviders    = allMetadata.SelectMany(m => m.GetTokenContextProvider(tokenPool)).ToList();

            this.ReportBuilders = allMetadata.SelectMany(m => m.GetReportBuilders()).ToArray();
        }

        private void CheckAllScanRulesDefined(List<TokenRef> undefinedTerminals, Type member, ILogging logging)
        {
            if (undefinedTerminals.Count == 0)
            {
                return;
            }

            var message = new StringBuilder("Undefined scan or parse rules for tokens: ");
            bool first = true;
            foreach (var term in undefinedTerminals)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    message.Append(", ");
                }

                message.Append(term.TokenType.Name);
            }

            logging.Write(
                new LogEntry
                {
                    Severity = Severity.Warning,
                    Message  = message.ToString(),
                    Member   = member,
                });
        }

        public bool IsValid { get; private set; }

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

        public ReportBuilder[] ReportBuilders { get; private set; }

        public ITokenRefResolver TokenRefResolver { get; private set; }

        public IEnumerable<TokenFeature<Precedence>> Precedence { get { return precedence; } }

        public IList<ParseRule> ParseRules { get { return allParseRules; } }

        public IList<MergeRule> MergeRules { get { return allMergeRules; } }

        public IList<ScanMode> ScanModes { get { return allScanModes; } }

        public IList<TokenFeature<ContextProvider>> ContextProviders { get; private set; }

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
