using System.Collections.Generic;
using System.Linq;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Misc;
using IronText.Reflection;

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
        private readonly List<ICilMetadata> validMetadata   = new List<ICilMetadata>();
        private readonly List<ICilMetadata> invalidMetadata = new List<ICilMetadata>();
        private readonly List<CilProduction>         allParseRules   = new List<CilProduction>();
        private readonly List<CilSymbolRef>          allTokens       = new List<CilSymbolRef>();

        private readonly ILogging                logging;

        public MetadataCollector(ILogging logging)
        {
            this.logging = logging;
        }

        public bool HasInvalidData { get { return invalidMetadata.Count != 0; } }

        public List<ICilMetadata> AllMetadata { get { return validMetadata; } } 

        public List<CilProduction> AllParseRules { get { return allParseRules; } } 

        public List<CilSymbolRef> AllTokens { get { return allTokens; } } 

        public void AddMeta(ICilMetadata meta)
        {
            if (validMetadata.Contains(meta, PropertyComparer<ICilMetadata>.Default)
                ||
                invalidMetadata.Contains(meta, PropertyComparer<ICilMetadata>.Default))
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
            foreach (var token in meta.GetSymbolsInCategory(SymbolCategory.ExplicitlyUsed))
            {
                this.AddToken(token);
            }

            // Provide new rules
            var newParseRules = meta.GetProductions(EnumerateSnapshot(allTokens));
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

        public void AddRule(ICilMetadata meta, CilProduction parseRule)
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

        public void AddToken(CilSymbolRef token)
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
                var newParseRules = meta.GetProductions(newTokens);
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
