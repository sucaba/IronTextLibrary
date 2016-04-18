using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Runtime.Semantics
{
    class RuntimeShiftSemantics
    {
        private readonly RuntimeGrammar grammar;

        public RuntimeShiftSemantics(RuntimeGrammar grammar)
        {
            this.grammar = grammar;
        }

        public void Execute(IStackLookback<ActionNode> lookback)
        {
            int shiftedState = lookback.GetParentState();
            RuntimeFormula[] formulas = grammar.GetShiftedFormulas(shiftedState);
            foreach (var formula in formulas)
            {
                formula.Execute(lookback);
            }
        }
    }
}
