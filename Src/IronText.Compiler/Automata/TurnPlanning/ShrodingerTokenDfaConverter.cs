using IronText.Collections;
using IronText.Common;
using System.Linq;

namespace IronText.Automata.TurnPlanning
{
    class ShrodingerTokenDfaConverter
    {
        public GraphImplMap<TokenDfaState, ShrodingerTokenDfaState> StateMap { get; }

        public ShrodingerTokenDfaConverter(TokenDfaProvider tokenDfa)
        {
            this.StateMap = new GraphImplMap<TokenDfaState, ShrodingerTokenDfaState>(Init);

            StateMap.EnsureMapped(tokenDfa.States);
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
