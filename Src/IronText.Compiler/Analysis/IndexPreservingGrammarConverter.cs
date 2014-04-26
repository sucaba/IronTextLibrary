using System;
using IronText.Reflection;

namespace IronText.Analysis
{
    class IndexPreservingGrammarConverter : IGrammarConverter
    {
        private readonly Grammar destination;

        public IndexPreservingGrammarConverter(Grammar source, Grammar destination)
        {
            this.destination = destination;

            foreach (var srcSymbol in source.Symbols)
            {
                // Ensure destSymbol exists
                var destSymbol = Convert(srcSymbol);

                if (source.Start == srcSymbol)
                {
                    destination.Start = (Symbol)destSymbol;
                }
            }
        }

        public T Convert<T>(T source) where T : SymbolBase
        {
            T result;

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            int index = source.Index;
            if (index < 0)
            {
                throw new InvalidOperationException("Unable to convert unindexed symbol.");
            }

            if (index < destination.Symbols.Count)
            {
                // Symbol already exists
                return (T)destination.Symbols[index];
            }
            else
            {
                result = (T)source.Clone();
                destination.Symbols[index] = result;
            }

            return result;
        }

        public Production Convert(Production source)
        {
            var outcome = Convert(source.Outcome);
            var pattern = Array.ConvertAll(source.Pattern, Convert);

            var result = new Production(outcome, pattern, source.ContextRef)
            {
                ExplicitPrecedence = source.ExplicitPrecedence,
            };

            return result;
        }
    }
}
