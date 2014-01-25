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
        private readonly List<ICilMetadata>     validMetadata;
        private readonly List<ICilMetadata>     invalidMetadata;
        private readonly List<CilScanCondition> allScanConditions;
        private readonly List<CilSymbolRef>     terminals;
        private readonly ITokenPool             tokenPool;

        private readonly Stack<CilScanCondition> processedScanConditions;
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
            this.allScanConditions = new List<CilScanCondition>();
            this.terminals    = new List<CilSymbolRef>(terminals);
            this.voidTerm = tokenPool.ScanSkipToken;
            //this.terminals.Add(voidTerm);

            this.tokenPool    = tokenPool;

            processedScanConditions = new Stack<CilScanCondition>();
        }

        public List<CilSymbolRef> UndefinedTerminals
        {
            get
            {
                return 
                    terminals
                    .Except(
                        allScanConditions
                        .SelectMany(mode => mode.ScanRules)
                        .SelectMany(rule => rule.GetAllOutcomes())
                        .Distinct())
                    .ToList();
            }
        }

        public bool HasInvalidData { get { return invalidMetadata.Count != 0; } }

        public List<CilScanCondition> ScanConditions { get { return allScanConditions; } }

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
            var currentScanMode = processedScanConditions.Peek();
            currentScanMode.AddRule(rule);

            if (rule.NextModeType != null)
            {
                AddScanMode(rule.NextModeType);
            }
        }

        public void AddScanMode(Type modeType)
        {
            if (allScanConditions.Any(mode => object.Equals(mode.ScanModeType, modeType)))
            {
                return;
            }

            var scanMode = new CilScanCondition(modeType);

            allScanConditions.Add(scanMode);

            processedScanConditions.Push(scanMode);

            foreach (var meta in MetadataParser.EnumerateAndBind(modeType))
            {
                AddMeta(meta);
            }

            if (processedScanConditions.Count == 1)
            {
                var implicitLiterals = 
                    (from t in terminals
                     where t.IsLiteral
                     select t.LiteralText)
                    .Except(
                        from mode in allScanConditions
                        from rule in mode.ScanRules
                        let singleTokenRule = (rule as ICilSingleTokenScanRule)
                        where singleTokenRule != null
                            && singleTokenRule.LiteralText != null
                        select singleTokenRule.LiteralText)
                        .ToArray();

                foreach (var literal in implicitLiterals)
                {
                    scanMode.AddImplicitLiteralRule(literal);
                }
            }

            processedScanConditions.Pop();
        }
    }
}
