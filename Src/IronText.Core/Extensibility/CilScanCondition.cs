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
        private readonly List<CilScanProduction> productions = new List<CilScanProduction>();
        private int implicitRulesCount = 0;

        public CilScanCondition(Type conditionType)
        {
            this.ConditionType = conditionType;
            this.Productions   = new ReadOnlyCollection<CilScanProduction>(this.productions);
        }

        public Type ConditionType { get; private set; }

        // Ordered scan rules
        public ReadOnlyCollection<CilScanProduction> Productions { get; private set; }

        internal CilScanProduction AddImplicitLiteralRule(string literal)
        {
            var result = CreateImplicitLiteralRule(literal);
            productions.Insert(implicitRulesCount++, result);

            return result;
        }

        internal void AddRule(CilScanProduction rule)
        {
            productions.Add(rule);
        }

        private static CilScanProduction CreateImplicitLiteralRule(string literal)
        {
            var outcome = CilSymbolRef.Literal(literal);

            // Generate implicit scan rule for the keyword
            var result  = new CilScanProduction
            {
                MainOutcome      = outcome,
                AllOutcomes      = { outcome },
                Disambiguation   = Disambiguation.Exclusive,
                Pattern      = ScanPattern.CreateLiteral(literal),
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
