using System.Collections.Generic;
using System.Linq;
using IronText.Extensibility;
using IronText.Extensibility.Cil;
using IronText.Framework;
using IronText.Misc;

namespace IronText.MetadataCompiler
{
    /* Collector with recursive logic:
        --------------------------------------------------------
        event     => effect
        --------------------------------------------------------
        new meta  => can provide new meta-children tree
        new meta  => can provide new rules
        new meta  => can provide new explicitly used tokens
        new rule  => can provide new tokens
        new token => can provide new rules from existing meta
        new token => can provide new meta (DemandAttribute)
    */
    class MetadataCollector : IMetadataCollector
    {
        private readonly List<ILanguageMetadata> validMetadata   = new List<ILanguageMetadata>();
        private readonly List<ILanguageMetadata> invalidMetadata = new List<ILanguageMetadata>();
        private readonly List<CilProductionDef>         allParseRules   = new List<CilProductionDef>();
        private readonly List<TokenRef>          allTokens       = new List<TokenRef>();

        private readonly ITokenPool              tokenPool;
        private readonly ILogging                logging;

        public MetadataCollector(ITokenPool tokenPool, ILogging logging)
        {
            this.tokenPool = tokenPool;
            this.logging = logging;
        }

        public bool HasInvalidData { get { return invalidMetadata.Count != 0; } }

        public List<ILanguageMetadata> AllMetadata { get { return validMetadata; } } 

        public List<CilProductionDef> AllParseRules { get { return allParseRules; } } 

        public List<TokenRef> AllTokens { get { return allTokens; } } 

        public void AddMeta(ILanguageMetadata meta)
        {
            if (validMetadata.Contains(meta, PropertyComparer<ILanguageMetadata>.Default)
                ||
                invalidMetadata.Contains(meta, PropertyComparer<ILanguageMetadata>.Default))
            {
                return; 
            }

            if (!meta.Validate(logging))
            {
                invalidMetadata.Add(meta);
                return;
            }

            validMetadata.Add(meta);

            // Provide new explicitly used tokens
            foreach (var token in meta.GetTokensInCategory(tokenPool, SymbolCategory.ExplicitlyUsed))
            {
                this.AddToken(token);
            }

            // Provide new rules
            var newParseRules = meta.GetProductions(EnumerateSnapshot(allTokens), tokenPool);
            foreach (var parseRule in newParseRules)
            {
                this.AddRule(meta, parseRule);
            }

            // Provide new meta children
            foreach (var childMeta in meta.GetChildren())
            {
                this.AddMeta(childMeta);
            }
        }

        public void AddRule(ILanguageMetadata meta, CilProductionDef parseRule)
        {
            if (parseRule.Owner == meta || allParseRules.Any(r => r.Owner == meta && r.Equals(parseRule)))
            {
                return;
            }

            parseRule.Owner = meta;

            allParseRules.Add(parseRule);

            // Provide new tokens
            foreach (var part in parseRule.Parts)
            {
                this.AddToken(part);
            }

            this.AddToken(parseRule.Left);
        }

        public void AddToken(TokenRef token)
        {
            if (allTokens.Contains(token))
            {
                return;
            }

            allTokens.Add(token);

            // Provide new rules from existing meta
            var newTokens = new[] { token };
            foreach (var meta in validMetadata)
            {
                var newParseRules = meta.GetProductions(newTokens, tokenPool);
                foreach (var parseRule in newParseRules)
                {
                    this.AddRule(meta, parseRule);
                }
            }

            // Provide new meta (DemandAttribute)
            if (!token.IsLiteral)
            {
                foreach (var meta in MetadataParser.EnumerateAndBind(token.TokenType))
                {
                    this.AddMeta(meta);
                }
            }
        }

        private static IEnumerable<T> EnumerateSnapshot<T>(IList<T> items)
        {
            int count = items.Count;
            for (int i = 0; i != count; ++i)
            {
                yield return items[i];
            }
        }
    }
}
