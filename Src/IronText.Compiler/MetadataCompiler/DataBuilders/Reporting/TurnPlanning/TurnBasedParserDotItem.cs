using IronText.Automata.TurnPlanning;
using IronText.Reflection;
using IronText.Reporting;
using System.Collections.Generic;
using System.Linq;

namespace IronText.MetadataCompiler.DataBuilders.Reporting.TurnPlanning
{
    class TurnBasedParserDotItem : IParserDotItem
    {
        public TurnBasedParserDotItem(
            TurnBasedNameProvider nameProvider,
            PlanPosition planPosition,
            IEnumerable<string> lookaheads)
        {
            Input = planPosition
                .Plan
                .KnownTurns
                .Select(nameProvider.NameOfTurn)
                .ToArray();

            InputLength = Input.Length;

            Outcome     = nameProvider.NameOfSymbol(planPosition.Plan.Outcome);

            Position    = planPosition.Position;

            LA          = lookaheads.ToArray();
        }

        public string[] Input       { get; }

        public int      InputLength { get; }
        
        public string   Outcome     { get; }

        public int      Position    { get; }

        public int      ProductionIndex => 0;

        public IEnumerable<string> LA { get; }
    }
}
