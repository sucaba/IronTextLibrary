using IronText.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IronText.Automata.TurnPlanning
{
    class ShrodingerTokenDfaProvider
    {
        public ShrodingerTokenDfaState[] States { get; }

        public IReadOnlyDictionary<ShrodingerTokenDfaState, TurnDfaStateDetails> Details { get; }

        public ShrodingerTokenDfaProvider(
            ShrodingerTokenDfaConverter converter,
            AmbTokenInfo[]              ambiguities)
        {
            this.States  = converter.StateMap.Destinations.ToArray();
            this.Details = converter.Details;

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
