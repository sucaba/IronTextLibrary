using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Runtime.Semantics
{
    class RuntimeReduceSemantics
    {
        private readonly RuntimeGrammar grammar;

        public RuntimeReduceSemantics(RuntimeGrammar grammar)
        {
            this.grammar = grammar;
        }

        class ProdNode
        {
            public int               ProductionIndex { get; set; }

            public RuntimeProduction Production      { get; set; }

            public ProdNode[]        Components      { get; set; }
        }

        public void Execute(int prodIndex, IStackLookback<ActionNode> lookback, ActionNode outcomeNode)
        {
            RuntimeFormula[] formulas = grammar.GetReduceFormulas(prodIndex);
            foreach (var formula in formulas)
            {
                formula.Execute(lookback, outcomeNode);
            }
        }

        void ExcecuteInlinedTree(ProdNode node)
        {
            // var varStack = new ActionNode[100];
        }
    }
}
