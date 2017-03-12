using IronText.Runtime;
using System.Diagnostics;

namespace IronText.Automata.TurnPlanning
{
    class AcceptanceTurn : Turn
    {
        public override int? TokenToConsume => PredefinedTokens.Eoi;

        public override bool Equals(Turn other) =>
            other is AcceptanceTurn;

        public override int GetHashCode() => PredefinedTokens.Eoi;

        public override string ToString() => "accept";
    }
}
