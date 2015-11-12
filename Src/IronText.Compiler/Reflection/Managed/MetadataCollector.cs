using System.Collections.Generic;
using System.Linq;
using IronText.Extensibility;
using IronText.Logging;
using IronText.Misc;
using IronText.Runtime;

namespace IronText.Reflection.Managed
{
    /* Collector with recursive logic:
        --------------------------------------------------------
        event     => effect
        --------------------------------------------------------
        new meta  => can provide new meta-children tree
        new meta  => can provide new prodss
        new meta  => can provide new explicitly used tokens
        new prod  => can provide new tokens
        new token => can provide new prods from existing meta
        new token => can provide new meta (DemandAttribute)
    */
    class MetadataCollector : IMetadataCollector
    {
        private readonly List<ICilMetadata>  validMetadata   = new List<ICilMetadata>();
        private readonly List<ICilMetadata>  invalidMetadata = new List<ICilMetadata>();
        private readonly List<CilProduction> productions     = new List<CilProduction>();
        private readonly List<CilSymbolRef>  symbols         = new List<CilSymbolRef>();
        private readonly ILogging            logging;

        public MetadataCollector(ILogging logging)
        {
            this.logging = logging;
        }

        public bool HasInvalidData { get { return invalidMetadata.Count != 0; } }

        public List<ICilMetadata>  Metadata    { get { return validMetadata; } } 

        public List<CilProduction> Productions { get { return productions; } } 

        public List<CilSymbolRef>  Symbols     { get { return symbols; } } 

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
                this.AddSymbol(token);
            }

            // Provide new rules
            var newParseRules = meta.GetProductions(EnumerateSnapshot(symbols));
            foreach (var parseRule in newParseRules)
            {
                this.AddProduction(meta, parseRule);
            }

            // Provide new meta children
            foreach (var childMeta in meta.GetChildren())
            {
                this.AddMeta(childMeta);
            }
        }

        public void AddProduction(ICilMetadata meta, CilProduction parseRule)
        {
            if (parseRule.Owner == meta || productions.Any(r => r.Owner == meta && r.Equals(parseRule)))
            {
                return;
            }

            parseRule.Owner = meta;

            productions.Add(parseRule);

            // Provide new tokens
            foreach (var part in parseRule.Pattern)
            {
                this.AddSymbol(part);
            }

            this.AddSymbol(parseRule.Outcome);
        }

        public void AddSymbol(CilSymbolRef token)
        {
            if (symbols.Contains(token))
            {
                return;
            }

            symbols.Add(token);

            // Provide new rules from existing meta
            var newTokens = new[] { token };
            foreach (var meta in validMetadata)
            {
                var newParseRules = meta.GetProductions(newTokens);
                foreach (var parseRule in newParseRules)
                {
                    this.AddProduction(meta, parseRule);
                }
            }

            // Provide new meta (DemandAttribute)
            if (!token.HasLiteral)
            {
                foreach (var meta in MetadataParser.EnumerateAndBind(token.Type))
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
