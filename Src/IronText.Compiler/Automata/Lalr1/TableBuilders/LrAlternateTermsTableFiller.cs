using IronText.Compiler.Analysis;
using IronText.Runtime;
using System.Linq;

namespace IronText.Automata.Lalr1
{
    class LrAlternateTermsTableFiller
    {
        private readonly DotState[] states;
        private readonly AmbTokenInfo[] ambiguities;

        public LrAlternateTermsTableFiller(ILrDfa lrDfa, AmbTokenInfo[] ambiguities)
        {
            this.states = lrDfa.States;
            this.ambiguities = ambiguities;
        }

        public bool Apply(LrTableBuilder builder)
        {
            bool result = false;

            for (int i = 0; i != states.Length; ++i)
            {
                var state = states[i];

                foreach (var ambiguousTerm in ambiguities)
                {
                    var tokenToDecision = builder.GetMultiple(i, ambiguousTerm.Alternatives);

                    switch (tokenToDecision.Values.Distinct().Count())
                    {
                        case 0:
                            // AmbToken is entirely non-acceptable for this state
                            builder.AssignAction(
                                i,
                                ambiguousTerm.EnvelopeIndex,
                                ParserDecision.NoAlternatives);
                            break;
                        case 1:
                            {
                                var pair = tokenToDecision.First();
                                if (pair.Key == ambiguousTerm.MainToken)
                                {
                                    // ambiguousToken action is the same as for the main token
                                    builder.AssignAction(
                                        i,
                                        ambiguousTerm.EnvelopeIndex,
                                        pair.Value);
                                }
                                else
                                {
                                    // Resolve ambToken to a one of the underlying tokens.
                                    // In runtime transition will be acceptable when this token
                                    // is in Msg and non-acceptable when this particular token
                                    // is not in Msg.
                                    var action = ParserInstruction.Resolve(pair.Key);
                                    builder.AssignAction(
                                        i,
                                        ambiguousTerm.EnvelopeIndex,
                                        action);
                                }
                            }

                            break;
                        default:
                            // GLR parser is required to handle terminal token alternatives.
                            result = true;

                            // No needed for GLR but but for the sake of explicitness
                            builder.AssignAction(
                                i,
                                ambiguousTerm.EnvelopeIndex,
                                ParserDecision.NoAlternatives);
                            break;
                    }
                }
            }

            return result;
        }
    }
}
