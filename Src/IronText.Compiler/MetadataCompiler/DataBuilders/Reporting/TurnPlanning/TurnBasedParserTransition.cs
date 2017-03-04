using IronText.Reporting;
using System.Collections.Generic;

namespace IronText.MetadataCompiler.DataBuilders.Reporting.TurnPlanning
{
    class TurnBasedParserTransition : IParserTransition
    {
        public TurnBasedParserTransition(string symbol, IEnumerable<IParserDecision> decisions)
        {
            Symbol             = symbol;
            AlternateDecisions = decisions;
        }

        public string                       Symbol             { get; }

        public IEnumerable<IParserDecision> AlternateDecisions { get; }
    }
}
