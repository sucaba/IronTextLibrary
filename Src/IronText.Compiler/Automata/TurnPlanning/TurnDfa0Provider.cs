using IronText.Common;
using System.Collections.Generic;
using System.Linq;

namespace IronText.Automata.TurnPlanning
{
    class TurnDfa0Provider
    {
        public TurnDfaState[] States { get; }

        public ImplMap<TurnDfaState, TurnDfaStateDetails> Details { get; }

        private readonly TurnNfa0Provider nfa;

        public TurnDfa0Provider(TurnNfa0Provider nfa)
        {
            this.nfa = nfa;
            this.Details = new ImplMap<TurnDfaState, TurnDfaStateDetails>(
                            s => new TurnDfaStateDetails(s));

            var start = new TurnDfaState();
            Details.Of(start).SetPositions(nfa.Start);

            var result = new List<TurnDfaState>() { start };

            bool modified;
            do
            {
                modified = false;
                foreach (var fromState in result.EnumerateGrowable())
                {
                    foreach (var transition in nfa.Transitions(Details.Of(fromState).Positions))
                    {
                        var existing = result.Find(state =>
                            Enumerable.SequenceEqual(
                                Details.Of(state).Positions,
                                transition.NextPostions));
                        if (existing == null)
                        {
                            existing = new TurnDfaState();
                            result.Add(existing);
                            Details.Of(existing).SetPositions(transition.NextPostions);
                            modified = true;
                        }

                        fromState.AddTransition(transition.Turn, existing);
                    }
                }
            }
            while (modified);

            this.States = result.ToArray();
        }
    }
}
