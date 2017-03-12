using IronText.Common;
using System.Collections.Generic;

namespace IronText.Automata.TurnPlanning
{
    class TurnDfaState
    {
        public static readonly TurnDfaState Fail = new TurnDfaState();

        public Dictionary<Turn, TurnDfaState> Transitions { get; }
            = new Dictionary<Turn, TurnDfaState>();

        public IReadOnlyCollection<Turn> Turns => Transitions.Keys;

        public TurnDfaState GetNext(Turn turn) => Transitions.GetOrDefault(turn, Fail);

        public void AddTransition(Turn turn, TurnDfaState next)
        {
            Transitions.Add(turn, next);
        }

        public override string ToString() => GetHashCode().ToString();
    }
}
