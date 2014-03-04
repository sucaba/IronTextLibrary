using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace IronText.Reflection.Managed
{
    internal class CilCondition
    {
        private readonly List<CilMatcher> matchers = new List<CilMatcher>();
        private int implicitLiteralCount = 0;

        public CilCondition(Type conditionType)
        {
            this.ConditionType   = conditionType;
            this.Matchers        = new ReadOnlyCollection<CilMatcher>(this.matchers);
            this.ContextProvider = new CilContextProvider(conditionType);
        }

        public Type ConditionType { get; private set; }

        public CilContextProvider ContextProvider { get; private set; }
        
        // Ordered scan rules
        public ReadOnlyCollection<CilMatcher> Matchers { get; private set; }

        internal CilMatcher AddImplicitLiteralMatcher(string literal)
        {
            var result = CreateImplicitLiteralMatcher(literal);
            matchers.Insert(implicitLiteralCount++, result);
            return result;
        }

        internal void AddMatcher(CilMatcher matcher)
        {
            matchers.Add(matcher);
        }

        private static CilMatcher CreateImplicitLiteralMatcher(string literal)
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
