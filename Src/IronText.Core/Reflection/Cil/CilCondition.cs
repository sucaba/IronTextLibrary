using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace IronText.Reflection.Managed
{
    internal class CilCondition
    {
        private readonly List<CilMatcher> productions = new List<CilMatcher>();
        private int implicitRulesCount = 0;

        public CilCondition(Type conditionType)
        {
            this.ConditionType = conditionType;
            this.Productions   = new ReadOnlyCollection<CilMatcher>(this.productions);
        }

        public Type ConditionType { get; private set; }

        // Ordered scan rules
        public ReadOnlyCollection<CilMatcher> Productions { get; private set; }

        internal CilMatcher AddImplicitLiteralRule(string literal)
        {
            var result = CreateImplicitLiteralRule(literal);
            productions.Insert(implicitRulesCount++, result);

            return result;
        }

        internal void AddRule(CilMatcher rule)
        {
            productions.Add(rule);
        }

        private static CilMatcher CreateImplicitLiteralRule(string literal)
        {
            var outcome = CilSymbolRef.Create(literal);

            // Generate implicit scan rule for the keyword
            var result  = new CilMatcher
            {
                MainOutcome     = outcome,
                AllOutcomes     = { outcome },
                Disambiguation  = Disambiguation.Exclusive,
                Pattern         = ScanPattern.CreateLiteral(literal),
                ActionBuilder   = code =>
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
