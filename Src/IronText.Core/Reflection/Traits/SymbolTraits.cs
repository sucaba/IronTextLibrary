using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Reflection
{
    public static class SymbolTraits
    {
        public const string SomeSymbolSuffix = "_d$";

        public static bool HasEmptyProduction(this Symbol symbol)
        {
            return symbol.Productions.Any(ProductionTraits.HasEmptyInput);
        }

        /// <summary>
        /// Defines symbol which has only empty production and unary production.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static bool IsOptionalSymbol(this Symbol symbol)
        {
            if (symbol == null)
            {
                return false;
            }

            var prods = symbol.Productions;
            bool result = prods.Count == 2
                        && prods.Any(p => p.Input.Length == 0)
                        && prods.Any(p => p.Input.Length == 1);
            return result;
        }

        /// <summary>
        /// Get 'some' symbol of opt-symbol.
        /// </summary>
        /// <param name="optSymbol"></param>
        /// <returns></returns>
        public static Symbol Some(this Symbol optSymbol)
        {
#if DEBUG
            if (!optSymbol.IsOptionalSymbol())
            {
                throw new ArgumentException("Expected optional-symbol.", "optSymbol");
            }
#endif

            var prod = optSymbol.Productions.First();
            if (prod.Input.Length == 0)
            {
                prod = optSymbol.Productions.Last();
            }

            return prod.Input[0];
        }
    }
}
