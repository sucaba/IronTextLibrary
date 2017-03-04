using IronText.Collections;
using IronText.Common;
using System.Collections.Generic;
using System.Linq;

namespace IronText.Automata.TurnPlanning
{
    class TokenDfaProvider
    {
        public TokenDfaState[] States { get; }

        public IReadOnlyDictionary<TokenDfaState,TurnDfaStateDetails> Details { get; }

        public TokenDfaProvider(
            TurnDfa1Provider       turnDfa1,
            TurnDfa1FirstsProvider firstsProvider)
        {
            var firsts = firstsProvider.Firsts;

            var tokenStates = new ImplMap<TurnDfaState, TokenDfaState>(
                            s => new TokenDfaState());

            foreach (var turnState in turnDfa1.States)
            {
                var state = tokenStates.Of(turnState);

                foreach (var token in firsts.Of(turnState))
                {
                    var tokenTurns = turnState
                        .Transitions
                        .Where(t => t.Key.Consumes(token)
                                 || (!t.Key.IsConsuming && firsts.Of(t.Value).Contains(token)))
                        .Select(t => t.Key);

                    var decision = TokenDecision.NoAlternatives;

                    foreach (var turn in tokenTurns)
                    {
                        var nextTurnState = turnState.Transitions[turn];
                        var nextState = tokenStates.Of(nextTurnState);
                        decision = decision.Alternate(
                                    new TokenDecision(turn, nextState));
                    }

                    state.Transitions.Add(token, decision);
                }
            }

            this.States  = tokenStates.Destinations.ToArray();
            this.Details = turnDfa1.States.ToDictionary(tokenStates.Of, turnDfa1.Details.Of);
        }
    }
}
