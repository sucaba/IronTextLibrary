using System.Collections.Generic;
using System.Linq;
using IronText.Extensibility;
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
        new token => can provide new meta (ThisAsTokenAttribute)
    */
    class MetadataCollector : IMetadataCollector
    {
        private readonly List<ILanguageMetadata> allMetadata    = new List<ILanguageMetadata>();
        private readonly List<ParseRule>         allParseRules  = new List<ParseRule>();
        private readonly List<SwitchRule>        allSwitchRules = new List<SwitchRule>();
        private readonly TokenCollection         allTokens      = new TokenCollection();

        private readonly ITokenPool              tokenPool;
        private readonly ILogging logging;

        public MetadataCollector(ITokenPool tokenPool, ILogging logging)
        {
            this.tokenPool = tokenPool;
            this.logging = logging;
        }

        public List<ILanguageMetadata> AllMetadata { get { return allMetadata; } } 

        public List<ParseRule> AllParseRules { get { return allParseRules; } } 

        public TokenCollection AllTokens { get { return allTokens; } } 

        public void AddMeta(ILanguageMetadata meta)
        {
            if (allMetadata.Contains(meta, PropertyComparer<ILanguageMetadata>.Default))
            {
                return; 
            }

            if (!meta.Validate(logging))
            {
                return;
            }

            allMetadata.Add(meta);

            // Provide new explicitly used tokens
            foreach (var token in meta.GetTokensInCategory(tokenPool, TokenCategory.ExplicitlyUsed))
            {
                this.AddToken(token);
            }

            // Provide new rules
            var newParseRules = meta.GetParseRules(allTokens, tokenPool);
            foreach (var parseRule in newParseRules)
            {
                this.AddRule(parseRule);
            }

            // Provide new meta children
            foreach (var childMeta in meta.GetChildren())
            {
                this.AddMeta(childMeta);
            }
        }

        public void AddRule(ParseRule parseRule)
        {
            if (allParseRules.Contains(parseRule))
            {
                return;
            }

            allParseRules.Add(parseRule);

            // Provide new tokens
            foreach (var part in parseRule.Parts)
            {
                this.AddToken(part);
            }

            this.AddToken(parseRule.Left);
        }

        public void AddSwitchRule(SwitchRule switchRule)
        {
            if (this.allSwitchRules.Contains(switchRule))
            {
                return;
            }

            allSwitchRules.Add(switchRule);
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
            foreach (var meta in allMetadata)
            {
                var newParseRules = meta.GetParseRules(newTokens, tokenPool);
                foreach (var parseRule in newParseRules)
                {
                    this.AddRule(parseRule);
                }
            }

            // Provide new meta (ThisAsTokenAttribute)
            if (!token.IsLiteral)
            {
                foreach (var meta in MetadataParser.EnumerateAndBind(token.TokenType))
                {
                    this.AddMeta(meta);
                }
            }
        }
    }
}
