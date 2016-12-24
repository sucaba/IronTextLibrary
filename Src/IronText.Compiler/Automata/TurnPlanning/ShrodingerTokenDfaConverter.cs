using IronText.Collections;
using IronText.Common;
using System.Linq;

namespace IronText.Automata.TurnPlanning
{
    class ShrodingerTokenDfaConverter
    {
        public ImplMap<TokenDfaState, ShrodingerTokenDfaState> StateMap { get; }

        public ShrodingerTokenDfaConverter(TokenDfaProvider tokenDfa)
        {
            this.StateMap = new ImplMap<TokenDfaState, ShrodingerTokenDfaState>(Convert);

            StateMap.EnsureMapped(tokenDfa.States);
        }

        public ShrodingerTokenDecision Convert(TokenDecision decision, int resolvedToken) =>
            decision
                .MapAltenatives(
                    src => new ShrodingerTokenDecision(
                        resolvedToken,
                        src.Turn,
                        StateMap[src.NextState]));

        private ShrodingerTokenDfaState Convert(TokenDfaState src) =>
            new ShrodingerTokenDfaState(
                src.Transitions.ToDictionary(
                    x => x.Key,
                    x => Convert(x.Value, x.Key)));
    }
}
