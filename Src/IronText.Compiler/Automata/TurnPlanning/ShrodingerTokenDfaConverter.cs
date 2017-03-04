using IronText.Collections;
using IronText.Common;
using System.Collections.Generic;
using System.Linq;

namespace IronText.Automata.TurnPlanning
{
    class ShrodingerTokenDfaConverter
    {
        public GraphImplMap<TokenDfaState, ShrodingerTokenDfaState> StateMap { get; }

        public IReadOnlyDictionary<ShrodingerTokenDfaState, TurnDfaStateDetails> Details { get; }

        public ShrodingerTokenDfaConverter(TokenDfaProvider tokenDfa)
        {
            this.StateMap = new GraphImplMap<TokenDfaState, ShrodingerTokenDfaState>(Init);

            StateMap.EnsureMapped(tokenDfa.States);

            var details = new Dictionary<ShrodingerTokenDfaState, TurnDfaStateDetails>();
            foreach (var state in tokenDfa.States)
            {
                details[StateMap.Of(state)] = tokenDfa.Details[state];
            }

            Details = details;
        }

        public ShrodingerTokenDecision Convert(TokenDecision decision, int resolvedToken) =>
            decision
                .MapAltenatives(
                    src => new ShrodingerTokenDecision(
                        resolvedToken,
                        src.Turn,
                        StateMap.Of(src.NextState)));

        private void Init(TokenDfaState src, ShrodingerTokenDfaState impl)
        {
            impl.Init(
                src.Transitions.ToDictionary(
                    x => x.Key,
                    x => Convert(x.Value, x.Key)));
        }
    }
}
