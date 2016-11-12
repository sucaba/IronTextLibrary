using IronText.Reflection.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IronText.Reflection.Transformations
{
    static class GrammarExtensions
    {
        public static void Inline(this Grammar @this)
        {
            @this.InlineNonAlternateAliasSymbols();
        }

        public static void InlineNonAlternateAliasSymbols(this Grammar @this)
        {
            var symbolsToInline = @this.Symbols.Where(IsNonAlternateAliasSymbol).ToArray();
            foreach (var symbol in symbolsToInline)
            {
                @this.Inline(symbol);
            }
        }

        public static void InlineEmptyProductions(this Grammar @this)
        {
            @this.ConvertNullableNonOptToOpt();
            @this.RecursivelyEliminateEmptyProductions();
        }

        internal static void RecursivelyEliminateEmptyProductions(this Grammar @this)
        {
            while (@this.EliminateEmptyProductions())
            {
            }
        }

        internal static bool EliminateEmptyProductions(this Grammar @this)
        {
            var old = @this.Productions.DuplicateResolver;
            @this.Productions.DuplicateResolver = ProductionDuplicateResolver.Instance;
            try
            {
                return @this.InternalEliminateEmptyProductions();
            }
            finally
            {
                @this.Productions.DuplicateResolver = old;
            }
        }

        private static bool InternalEliminateEmptyProductions(this Grammar @this)
        {
            bool result = false;

            var nullableSymbols = @this.FindNullableSymbols().ToArray();
            foreach (var symbol in nullableSymbols)
            {
                result = result || @this.Inline(symbol);
            }

            return result;
        }

        private static IEnumerable<Symbol> FindNullableSymbols(this Grammar @this)
        {
            var result = @this.Symbols
                    .Where(s => s.Productions.Count != 0
                             && s.Productions.Any(p => p.Input.Length == 0)
                             && !s.IsRecursive);

            return result;
        }

        private static bool IsNonAlternateAliasSymbol(Symbol symbol)
        {
            if (symbol == null 
                || symbol.IsPredefined 
                || symbol.IsStart
                || symbol.HasSideEffects)
            {
                return false;
            }

            var result =
                symbol.Productions.Count == 1 
                && (symbol.Productions.All(p => p.InputLength <= 1)
                    || 
                    symbol.Productions.All(
                        p => p.Input.All(s => s.IsTerminal)));

            if (result)
            {
                result = !symbol.IsRecursive;
            }

            return result;
        }

        public static bool Inline(this Grammar @this, Symbol symbol)
        {
            bool result = false;

            if (symbol == null)
            {
                throw new ArgumentNullException("symbol");
            }

            if (symbol.IsTerminal)
            {
                throw new ArgumentException("Cannot inline terminal symbol.", "symbol");
            }

            var productionsToExtend = @this.GetProductionsHavingInput(symbol).ToList();

            for (int i = 0; i != productionsToExtend.Count; ++i)
            {
                var prod = productionsToExtend[i];

                int pos = Array.IndexOf(prod.Input, symbol);
                if (pos >= 0)
                {
                    productionsToExtend.AddRange(@this.ExpandAt(prod, pos));
                    result = true;
                }
            }

            return result;
        }

        private static IEnumerable<Production> GetProductionsHavingInput(this Grammar @this, Symbol symbol)
        {
            foreach (var prod in @this.Productions)
            {
                if (prod.Input.Contains(symbol))
                {
                    yield return prod;
                }
            }
        }

        internal static IEnumerable<Production> ExpandAt(this Grammar @this, Production source, int position)
        {
            var result = new List<Production>();

            var symbol = source.Input[position];

            @this.Productions.SoftRemove(source);

            var producitonsToInline = symbol.Productions.ToArray();
            foreach (var inlinedProd in producitonsToInline)
            {
                var extended = new ProductionInliner(inlinedProd).Execute(source, position);
                @this.Productions.Add(extended);
                result.Add(extended);

                if (!inlinedProd.IsUsed)
                {
                    @this.Productions.SoftRemove(inlinedProd);
                }
            }

            return result;
        }

        public static Symbol Decompose(
            this Grammar          @this,
            Symbol                nonTerm,
            Func<Production,bool> criteria,
            string                newSymbolName)
        {
            Symbol newSymbol = @this.Symbols.Add(newSymbolName);
            newSymbol.Joint.AddAll(nonTerm.Joint);

            foreach (var prod in nonTerm.Productions.ToArray())
            {
                if (criteria(prod))
                {
                    var newProd = @this.Productions.Add(
                        new Production(newSymbol, prod));
                    newProd.ExplicitPrecedence = prod.ExplicitPrecedence;

                    newProd.Joint.AddAll(prod.Joint);

                    @this.Productions.SoftRemove(prod);
                }
            }

            @this.Productions.Add(new Production(nonTerm, newSymbol));

            return newSymbol;
        }

        public static Symbol[] FindOptionalPatternSymbols(this Grammar @this)
        {
            return @this.Symbols.Where(SymbolTraits.IsOptionalSymbol).ToArray();
        }

        public static bool InlineOptionalSymbols(this Grammar @this)
        {
            bool result = false;

            var symbolsToInline = @this.FindOptionalPatternSymbols();
            foreach (var symbol in symbolsToInline)
            {
                @this.Inline(symbol);
                result = true;
            }

            return result;
        }

        public static void ConvertNullableNonOptToOpt(this Grammar @this)
        {
            var nullableSymbols = @this.FindNullableNonOptSymbols();
            foreach (var symbol in nullableSymbols)
            {
                @this.Decompose(
                    symbol,
                    prod => !prod.Input.All(nullableSymbols.Contains),
                    symbol.Name + SymbolTraits.SomeSymbolSuffix);
            }
        }

        public static IEnumerable<Symbol> FindNullableNonOptSymbols(this Grammar @this)
        {
            var result = @this.Symbols
                   .Where(s => 
                       s.Productions.Any(p => p.Input.Length == 0)
                       && s.Productions.Any(p => p.Input.Length != 0)
                       && (s.Productions.Count != 2 
                          || s.Productions.Any(p => p.Input.Length > 1)));
            return result.ToArray();
        }
    }
}
