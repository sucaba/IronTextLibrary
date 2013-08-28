using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Misc;

namespace IronText.MetadataCompiler
{
    class ScanDataCollector : IScanDataCollector
    {
        private readonly List<ILanguageMetadata> allMetadata;
        private readonly List<ScanMode>          allScanModes;
        private readonly List<TokenRef>          terminals;
        private readonly ITokenPool              tokenPool;

        private readonly Stack<ScanMode> processedScanModes;
        private int rulePriority = 0;
        private readonly TokenRef voidTerm;
        private readonly ILogging logging;

        public ScanDataCollector(
            IEnumerable<TokenRef> terminals,
            ITokenPool tokenPool,
            ILogging logging)
        {
            this.logging = logging;
            this.allMetadata  = new List<ILanguageMetadata>();
            this.allScanModes = new List<ScanMode>();
            this.terminals    = new List<TokenRef>(terminals);
            this.voidTerm = tokenPool.ScanSkipToken;
            this.terminals.Add(voidTerm);

            this.tokenPool    = tokenPool;

            processedScanModes = new Stack<ScanMode>();
        }

        public List<ScanMode> ScanModes { get { return allScanModes; } }

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

            foreach (var scanRule in meta.GetScanRules(tokenPool))
            {
                /*
                var asSkipRule = scanRule as ISkipScanRule;
                if (asSkipRule != null)
                {
                    AddScanRule(scanRule);
                    continue;
                }

                var asSingleTokenScanRule = scanRule as ISingleTokenScanRule;
                if (asSingleTokenScanRule != null && terminals.Contains(asSingleTokenScanRule.SingleTokenRef))
                {
                    AddScanRule(scanRule);
                }
                */

                AddScanRule(scanRule);
            }

            foreach (var childMeta in meta.GetChildren())
            {
                this.AddMeta(childMeta);
            }
        }

        public void AddScanRule(ScanRule rule)
        {
            var currentScanMode = processedScanModes.Peek();
            currentScanMode.ScanRules.Add(rule);
            rule.Priority = rulePriority++;

            if (rule.NextModeType != null)
            {
                AddScanMode(rule.NextModeType);
            }
        }

        public void AddScanMode(Type modeType)
        {
            if (allScanModes.Any(mode => object.Equals(mode.ScanModeType, modeType)))
            {
                return;
            }

            var scanMode = new ScanMode
                {
                    ScanModeType = modeType,
                    ScanRules = new List<ScanRule>()
                };

            allScanModes.Add(scanMode);

            processedScanModes.Push(scanMode);

            foreach (var meta in MetadataParser.EnumerateAndBind(modeType))
            {
                AddMeta(meta);
            }

            if (processedScanModes.Count == 1)
            {
                var implicitLiterals = 
                    (from t in terminals
                     where t.IsLiteral
                     select t.LiteralText)
                    .Except(
                        from mode in allScanModes
                        from rule in mode.ScanRules
                        where rule is ISingleTokenScanRule
                            && rule.LiteralText != null
                        select rule.LiteralText)
                        .ToArray();

                foreach (var literal in implicitLiterals)
                {
                    scanMode.AddLiteralRule(literal);
                }
            }

            scanMode.SortScanRules();

            processedScanModes.Pop();
        }
    }
}
