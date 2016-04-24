using IronText.Compiler.Analysis;
using IronText.Misc;
using IronText.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.MetadataCompiler
{
    class MatchActionToTokenTableProvider
    {
        public MatchActionToTokenTableProvider(
            Grammar grammar,
            IEnumerable<AmbTokenInfo> ambiguities)
        {
            var actionToToken = grammar.Matchers.CreateCompatibleArray<int>(IndexingConstants.NoIndex);

            int first = grammar.Matchers.StartIndex;
            int last  = grammar.Matchers.Count;

            for (int i = first; i != last; ++i)
            {
                var outcome = grammar.Matchers[i].Outcome;
                if (outcome == null)
                {
                    actionToToken[i] = IndexingConstants.NoIndex; // Skip tokens like whitespace and comments
                }
                else if (outcome is AmbiguousTerminal)
                {
                    var ambOutcome = outcome as AmbiguousTerminal;
                    actionToToken[i] = (from amb in ambiguities
                                       where Enumerable.SequenceEqual(amb.Alternatives, ambOutcome.Alternatives.Select(alt => alt.Index))
                                       select amb.EnvelopeIndex)
                                       .SingleOrDefault();
                }
                else
                {
                    var detOutcome = (Symbol)outcome;
                    actionToToken[i] = detOutcome.Index;
                }
            }

            this.ActionToToken = actionToToken;
        }

        public int[] ActionToToken { get; }
    }
}
