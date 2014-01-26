using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using IronText.Algorithm;
using System.Collections.ObjectModel;
using IronText.Framework;
using IronText.Framework.Reflection;

namespace IronText.Extensibility
{
    internal class CilScanCondition
    {
        private readonly List<CilScanRule> scanRules = new List<CilScanRule>();
        private int implicitRulesCount = 0;

        internal CilScanCondition(Type scanModeType)
        {
            this.ScanModeType = scanModeType;
            this.ScanRules = new ReadOnlyCollection<CilScanRule>(this.scanRules);
        }

        public Type ScanModeType { get; private set; }

        // Ordered scan rules
        public ReadOnlyCollection<CilScanRule> ScanRules { get; private set; }

        internal CilScanRule AddImplicitLiteralRule(string literal)
        {
            var result = CreateImplicitLiteralRule(literal);
            scanRules.Insert(implicitRulesCount++, result);

            return result;
        }

        internal void AddRule(CilScanRule rule)
        {
            scanRules.Add(rule);
        }

        private static CilScanRule CreateImplicitLiteralRule(string literal)
        {
            var outcome = CilSymbolRef.Literal(literal);

            // Generate implicit scan rule for the keyword
            var result  = new CilScanRule
            {
                MainOutcome      = outcome,
                AllOutcomes      = { outcome },
                Disambiguation   = Disambiguation.Exclusive,
                ScanPattern      = ScanPattern.CreateLiteral(literal),
                Builder          = code =>
                {
                    code
                        .Emit(il => il.Ldnull())
                        .ReturnFromAction();
                }
            };

            return result;
        }
    }
}
