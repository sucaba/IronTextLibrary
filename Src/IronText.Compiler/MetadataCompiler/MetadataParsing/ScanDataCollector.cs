using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Misc;
using IronText.Algorithm;

namespace IronText.MetadataCompiler
{
    class ScanDataCollector : IScanDataCollector
    {
        private readonly List<ICilMetadata> validMetadata;
        private readonly List<ICilMetadata> invalidMetadata;
        private readonly List<ScanMode>          allScanModes;
        private readonly List<CilSymbolRef>          terminals;
        private readonly ITokenPool              tokenPool;

        private readonly Stack<ScanMode> processedScanModes;
        private int totalRuleCount = 0;
        private readonly CilSymbolRef voidTerm;
        private readonly ILogging logging;

        public ScanDataCollector(
            IEnumerable<CilSymbolRef> terminals,
            ITokenPool tokenPool,
            ILogging logging)
        {
            this.logging = logging;
            this.validMetadata  = new List<ICilMetadata>();
            this.invalidMetadata  = new List<ICilMetadata>();
            this.allScanModes = new List<ScanMode>();
            this.terminals    = new List<CilSymbolRef>(terminals);
            this.voidTerm = tokenPool.ScanSkipToken;
            //this.terminals.Add(voidTerm);

            this.tokenPool    = tokenPool;

            processedScanModes = new Stack<ScanMode>();
        }

        public List<CilSymbolRef> UndefinedTerminals
        {
            get
            {
                return 
                    terminals
                    .Except(
                        allScanModes
                        .SelectMany(mode => mode.ScanRules)
                        .SelectMany(rule => rule.GetTokenRefGroups())
                        .SelectMany(tokens => tokens)
                        .Distinct())
                    .ToList();
            }
        }

        public bool HasInvalidData { get { return invalidMetadata.Count != 0; } }

        public List<ScanMode> ScanModes { get { return allScanModes; } }

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

        public void AddScanRule(ICilScanRule rule)
        {
            var currentScanMode = processedScanModes.Peek();
            currentScanMode.AddRule(rule);
            rule.Index = totalRuleCount++;

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

            var scanMode = new ScanMode(modeType);

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
                        let singleTokenRule = (rule as ICilSingleTokenScanRule)
                        where singleTokenRule != null
                            && singleTokenRule.LiteralText != null
                        select singleTokenRule.LiteralText)
                        .ToArray();

                foreach (var literal in implicitLiterals)
                {
                    scanMode.AddImplicitLiteralRule(totalRuleCount++, literal);
                }
            }

            processedScanModes.Pop();
        }
    }
}
