using IronText.Collections;
using System.Linq;

namespace IronText.Automata.TurnPlanning
{
    class ShrodingerTokenDfaProvider
    {
        public ShrodingerTokenDfaState[] States { get; }

        public ShrodingerTokenDfaProvider(
            ShrodingerTokenDfaConverter converter,
            AmbTokenInfo[]              ambiguities)
        {
            this.States = converter.StateMap.Destinations.ToArray();

            foreach (var mapping in converter.StateMap)
            {
                var srcState = mapping.Key;
                var dstState = mapping.Value;

                foreach (var ambiguousTerm in ambiguities)
                {
                    dstState.Transitions.Add(
                        ambiguousTerm.EnvelopeIndex,
                        ambiguousTerm
                            .Alternatives
                            .Select(token => converter.Convert(srcState.GetDecision(token), token))
                            .Where(x => x != ShrodingerTokenDecision.NoAlternatives)
                            .AsAmbiguous());
                }
            }
        }
    }
}
