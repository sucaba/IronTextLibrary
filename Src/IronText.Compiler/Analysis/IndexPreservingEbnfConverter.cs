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

        public SymbolBase Convert(SymbolBase source)
        {
            SymbolBase result;

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
                return destination.Symbols[index];
            }
            else
            {
                result = (SymbolBase)source.Clone();
                destination.Symbols[index] = result;
            }

            return result;
        }

        public Symbol Convert(Symbol source)
        {
            return (Symbol)Convert((SymbolBase)source);
        }

        public AmbiguousSymbol Convert(AmbiguousSymbol source)
        {
            return (AmbiguousSymbol)Convert((SymbolBase)source);
        }

        public Production Convert(Production source)
        {
            var outcome = Convert(source.Outcome);
            var pattern = Array.ConvertAll(source.Pattern, Convert);

            return new Production(outcome, pattern)
            {
                ExplicitPrecedence = source.ExplicitPrecedence
            };
        }
    }
}
