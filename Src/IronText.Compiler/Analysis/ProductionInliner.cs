using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using IronText.Algorithm;
using IronText.Framework.Reflection;

namespace IronText.Analysis
{
    public class ProductionInliner : IProductionInliner
    {
        private readonly EbnfGrammar grammar;

        public ProductionInliner(EbnfGrammar sourceGrammar)
        {
            this.grammar = sourceGrammar;
        }

        public EbnfGrammar Inline()
        {
            var result = new EbnfGrammar();
            result.StartToken = grammar.StartToken;

            foreach (var srcSymbol in grammar.Symbols)
            {
                if (srcSymbol.IsPredefined)
                {
                    continue;
                }

                var destSymbol = (SymbolBase)srcSymbol.Clone();
                result.Symbols.Add(destSymbol);

                Debug.Assert(
                    destSymbol.Index == srcSymbol.Index,
                    "Internal Error: copied symbol index does not match.");
            }

            var symbolSetType = new BitSetType(grammar.Symbols.Count);

            var tokensToInline = symbolSetType.Of(
                                       from s in grammar.Symbols
                                       where CanInline(s as Symbol)
                                       select s.Index);
                                   
            var prodStack = new Stack<Tuple<int,Production>>(
                                from p in grammar.Productions
                                where !p.IsAugmented
                                select Tuple.Create(0, p.Clone()));
            
            while (prodStack.Count != 0)
            {
                var item = prodStack.Pop();
                var pos  = item.Item1;
                var prod = item.Item2;
                if (pos == prod.Size)
                {
                    if (!tokensToInline.Contains(prod.Outcome))
                    {
                        result.Productions.Add(prod);
                    }

                    continue;
                }

                var token = prod.Pattern[pos];
                if (!tokensToInline.Contains(token))
                {
                    prodStack.Push(Tuple.Create(pos + 1, prod));
                    continue;
                }

                foreach (var newProd in Inline(prod, pos))
                {
                    prodStack.Push(Tuple.Create(pos, newProd));
                }
            }

            return result;
        }

        private static bool CanInline(Symbol symbol)
        {
            if (symbol == null 
                || symbol.IsPredefined 
                || symbol.IsStart)
            {
                return false;
            }

            return symbol.Productions.Count == 1 
                && (symbol.Productions.All(p => p.Size <= 1)
                    || 
                    symbol.Productions.All(
                        p => p.PatternSymbols.All(
                                s => s.IsTerminal)));
        }

        public IEnumerable<Production> Inline(
            Production srcProduction,
            int        inlinePosition)
        {
            var inlinedSymbol = grammar.Symbols[srcProduction.Pattern[inlinePosition]];
            var oldPattern    = srcProduction.Pattern;

            foreach (var inlinedProd in inlinedSymbol.Productions)
            {
                var newPattern = new int[oldPattern.Length - 1 + inlinedProd.Pattern.Length];

                int pos = 0;

                // copy prefix
                for (; pos != inlinePosition; ++pos)
                {
                    newPattern[pos] = oldPattern[pos];
                }

                // copy inline
                for (int i = 0; i != inlinedProd.Size; ++i)
                {
                    newPattern[pos] = inlinedProd.Pattern[i];
                    ++pos;
                }

                // copy suffix
                for (int i = inlinePosition + 1; i != oldPattern.Length; ++i)
                {
                    newPattern[pos] = oldPattern[i];
                    ++pos;
                }

                var newProduction = new Production
                {
                    Outcome = srcProduction.Outcome,
                    Pattern = newPattern,
                };

                foreach (var action in srcProduction.Actions)
                {
                    newProduction.Actions.Add(action.Clone());
                }

                yield return newProduction;
            }
        }
    }
}
