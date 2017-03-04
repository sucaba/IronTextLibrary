using IronText.Automata.TurnPlanning;
using IronText.Reflection;
using IronText.Reporting;
using System.Collections.Generic;
using System.Linq;

namespace IronText.MetadataCompiler.DataBuilders.Reporting.TurnPlanning
{
    class TurnBasedParserDotItem : IParserDotItem
    {
        public TurnBasedParserDotItem(TurnBasedNameProvider nameProvider, PlanPosition planPosition)
        {
            Input = planPosition
                .Plan
                .KnownTurns
                .Select(nameProvider.NameOfTurn)
                .ToArray();

            InputLength = Input.Length;

            Outcome     = nameProvider.NameOfSymbol(planPosition.Plan.Outcome);

            Position    = planPosition.Position;
        }

        public string[] Input       { get; }

        public int      InputLength { get; }
        
        public string   Outcome     { get; }

        public int      Position    { get; }

        public int      ProductionIndex => 0;

        public IEnumerable<string> LA => Enumerable.Empty<string>();
    }
}
