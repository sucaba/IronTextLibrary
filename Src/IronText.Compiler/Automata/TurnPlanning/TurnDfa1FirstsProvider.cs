using System;
using System.Collections.Generic;
using System.Linq;

namespace IronText.Automata.TurnPlanning
{
    class TurnDfa1FirstsProvider
    {
        public TokenSetsRelation<TurnDfaState> Firsts { get; }

        public TurnDfa1FirstsProvider(TurnDfa1Provider dfa1, TokenSetProvider tokenSetProvider)
        {
            Firsts = new TokenSetsRelation<TurnDfaState>(tokenSetProvider.TokenSet);

            dfa1.ReturnLookaheads.CopyTo(Firsts);

            var returnStates = dfa1
                .ReturnLookaheads
                .Where(p => !p.Value.IsEmpty)
                .Select(p => p.Key)
                .ToArray();

            var nonReturnStates = dfa1.States.Except(returnStates);

            var toProcess = new Queue<TurnDfaState>(nonReturnStates.Reverse());

            int modified;

            do
            {
                modified = 0;

                foreach (var state in nonReturnStates)
                    foreach (var transition in state.Transitions)
                    {
                        var turn = transition.Key;
                        var next = transition.Value;
                        if (turn.TokenToConsume.HasValue)
                        {
                            if (Firsts.Add(state, turn.TokenToConsume.Value))
                            {
                                ++modified;
                            }
                        }
                        else
                        {
                            modified += Firsts.Add(state, Firsts.Of(next));
                        }
                    }
            }
            while (modified != 0);
        }
    }
}
