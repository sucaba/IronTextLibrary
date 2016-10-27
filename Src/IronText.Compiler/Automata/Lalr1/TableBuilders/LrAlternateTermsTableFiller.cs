using IronText.Compiler.Analysis;

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

        public int SymbolColumnCount => ambiguities.Length;

        public bool Apply(LrTableBuilder builder)
        {
            bool result = true;

            foreach (var state in states)
            {
                foreach (var ambiguousTerm in ambiguities)
                {
                    if (!builder.TryAssignResolution(
                            state.Index,
                            ambiguousTerm.EnvelopeIndex,
                            ambiguousTerm.Alternatives))
                    {
                        result = false;
                    }
                }
            }

            return result;
        }
    }
}
