using IronText.Reflection.Validation;
using System.Linq;

namespace IronText.Reflection.Transformations
{
    public sealed class EliminateRightNulls
    {
        private readonly Grammar grammar;

        public EliminateRightNulls(Grammar grammar)
        {
            this.grammar = grammar;
        }

        public void Apply()
        {
            var oldResolver = grammar.Productions.DuplicateResolver;
            grammar.Productions.DuplicateResolver = ProductionDuplicateResolver.Instance;
            try
            {
                InternalApply();
            }
            finally
            {
                grammar.Productions.DuplicateResolver = oldResolver;
            }
        }

        private void InternalApply()
        {
            while (true)
            {
                var prod = grammar.Productions.FirstOrDefault(ProductionTraits.HasEmptyInputTail);
                if (prod == null)
                {
                    break;
                }

                int pos  = prod.Input.Length - 1;
                /*
                var tail = prod.Input[pos];

                // Create optional symbol to reduce following expansion cost
                if (!SymbolTraits.IsOptionalSymbol(tail))
                {
                    grammar.Decompose(tail, p => p.Input.Length != 0, tail.Name + SymbolTraits.SomeSymbolSuffix);
                }
                */

                grammar.ExpandAt(prod, pos);
            }
        }
    }
}
