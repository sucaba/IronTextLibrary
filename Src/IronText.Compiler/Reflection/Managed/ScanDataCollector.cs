using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Misc;
using IronText.Algorithm;
using IronText.Logging;

namespace IronText.Reflection.Managed
{
    class ScanDataCollector : IScanDataCollector
    {
        private readonly List<ICilMetadata>     validMetadata;
        private readonly List<ICilMetadata>     invalidMetadata;
        private readonly List<CilScanCondition> allScanConditions;
        private readonly List<CilSymbolRef>     terminals;

        private readonly Stack<CilScanCondition> processedScanConditions;
        private readonly ILogging logging;

        public ScanDataCollector(IEnumerable<CilSymbolRef> terminals, ILogging logging)
        {
            this.logging = logging;
            this.validMetadata  = new List<ICilMetadata>();
            this.invalidMetadata  = new List<ICilMetadata>();
            this.allScanConditions = new List<CilScanCondition>();
            this.terminals    = new List<CilSymbolRef>(terminals);

            processedScanConditions = new Stack<CilScanCondition>();
        }

        public List<CilSymbolRef> Terminals { get { return terminals; } }

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

            foreach (var scanRule in meta.GetScanProductions())
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

                AddProduction(scanRule);
            }

            foreach (var childMeta in meta.GetChildren())
            {
                this.AddMeta(childMeta);
            }
        }

        public void AddProduction(CilScanProduction rule)
        {
            var currentScanMode = processedScanConditions.Peek();
            currentScanMode.AddRule(rule);

            if (rule.NextModeType != null)
            {
                AddCondition(rule.NextModeType);
            }
        }

        public void AddCondition(Type conditionType)
        {
            if (allScanConditions.Any(mode => object.Equals(mode.ConditionType, conditionType)))
            {
                return;
            }

            var scanMode = new CilScanCondition(conditionType);

            allScanConditions.Add(scanMode);

            processedScanConditions.Push(scanMode);

            foreach (var meta in MetadataParser.EnumerateAndBind(conditionType))
            {
                AddMeta(meta);
            }

            if (processedScanConditions.Count == 1)
            {
                var implicitLiterals = 
                    (from t in terminals
                     where t.HasLiteral
                     select t.Literal)
                    .Except(
                        from mode in allScanConditions
                        from rule in mode.Productions
                        where rule.Pattern.IsLiteral
                        select rule.Pattern.Literal)
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
