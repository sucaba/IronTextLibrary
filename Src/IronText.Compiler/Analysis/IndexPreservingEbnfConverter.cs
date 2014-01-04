using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using IronText.Framework.Reflection;

namespace IronText.Analysis
{
    class IndexPreservingEbnfConverter : IEbnfConverter
    {
        private readonly EbnfGrammar destination;

        public IndexPreservingEbnfConverter(EbnfGrammar destination)
        {
            this.destination = destination;
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

            var result = new Production(outcome, pattern)
            {
                ExplicitPrecedence = source.ExplicitPrecedence,
            };

            result.Action = source.Action.Clone();

            return result;
        }
    }
}
