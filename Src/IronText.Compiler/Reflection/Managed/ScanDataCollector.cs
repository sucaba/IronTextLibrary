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
        private readonly List<ICilMetadata> validMetadata;
        private readonly List<ICilMetadata> invalidMetadata;
        private readonly List<CilCondition> conditions;
        private readonly List<CilSymbolRef> terminals;

        private readonly Stack<CilCondition> processedConditions;
        private readonly ILogging logging;

        public ScanDataCollector(IEnumerable<CilSymbolRef> terminals, ILogging logging)
        {
            this.logging = logging;
            this.validMetadata   = new List<ICilMetadata>();
            this.invalidMetadata = new List<ICilMetadata>();
            this.conditions      = new List<CilCondition>();
            this.terminals       = new List<CilSymbolRef>(terminals);

            processedConditions = new Stack<CilCondition>();
        }

        public List<CilSymbolRef> Terminals { get { return terminals; } }

        public bool HasInvalidData { get { return invalidMetadata.Count != 0; } }

        public List<CilCondition> Conditions { get { return conditions; } }

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

            foreach (var matcher in meta.GetMatchers())
            {
                AddMatcher(matcher);
            }

            foreach (var childMeta in meta.GetChildren())
            {
                this.AddMeta(childMeta);
            }
        }

        public void AddMatcher(CilMatcher matcher)
        {
            var currentCondition = processedConditions.Peek();
            currentCondition.AddMatcher(matcher);

            if (matcher.NextConditionType != null)
            {
                AddCondition(matcher.NextConditionType);
            }
        }

        public void AddCondition(Type conditionType)
        {
            if (conditions.Any(mode => object.Equals(mode.ConditionType, conditionType)))
            {
                return;
            }

            var condition = new CilCondition(conditionType);

            conditions.Add(condition);

            processedConditions.Push(condition);

            foreach (var meta in MetadataParser.EnumerateAndBind(conditionType))
            {
                AddMeta(meta);
            }

            if (processedConditions.Count == 1)
            {
                var implicitLiterals = 
                    (from t in terminals
                     where t.HasLiteral
                     select t.Literal)
                    .Except(
                        from cond in conditions
                        from matcher in cond.Matchers
                        where matcher.Pattern.IsLiteral
                        select matcher.Pattern.Literal)
                        .ToArray();

                foreach (var literal in implicitLiterals)
                {
                    condition.AddImplicitLiteralMatcher(literal);
                }
            }

            processedConditions.Pop();
        }
    }
}
