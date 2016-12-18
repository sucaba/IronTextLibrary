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

        public ShrodingerTokenDecision Convert(int resolvedToken, TokenDecision decision) =>
            decision
                .MapAltenatives(
                    src => new ShrodingerTokenDecision(
                        resolvedToken,
                        src.Turn,
                        StateMap[src.NextState]));

        private ShrodingerTokenDfaState Convert(TokenDfaState srcState)
        {
            var result = new ShrodingerTokenDfaState();
            foreach (var transition in srcState.Transitions)
            {
                result.Transitions.Add(
                    transition.Key,
                    Convert(transition.Key, transition.Value));
            }

            return result;
        }
    }
}
