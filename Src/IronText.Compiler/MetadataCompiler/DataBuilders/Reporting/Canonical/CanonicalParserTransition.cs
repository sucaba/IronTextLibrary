using System.Collections.Generic;
using IronText.Runtime;
using IronText.Automata.Lalr1;
using System;
using IronText.Collections;
using System.Linq;

namespace IronText.Reflection.Reporting
{
    class CanonicalParserTransition : IParserTransition
    {
        private ParserDecision decision;

        public CanonicalParserTransition(
            CanonicalParserAutomata automata,
            string                  symbol,
            ParserDecision          decision)
        {
            this.Symbol    = symbol;
            this.decision = decision;
            this.AlternateDecisions = decision
                    .AllAlternatives()
                    .Select(d => new CanonicalParserDecision(automata, d));
        }

        public string         Symbol    { get; }

        public IEnumerable<IParserDecision> AlternateDecisions { get; }
    }
}
