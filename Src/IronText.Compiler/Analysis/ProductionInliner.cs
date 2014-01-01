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

            IEbnfConverter converter = new IndexPreservingEbnfConverter(result);
            var symbolsToInline = new HashSet<Symbol>();

            foreach (var srcSymbol in grammar.Symbols)
            {
                // Ensure destSymbol exists
                var destSymbol = converter.Convert(srcSymbol);

                if (CanInline(srcSymbol as Symbol))
                {
                    symbolsToInline.Add((Symbol)destSymbol);
                }

                if (grammar.Start == srcSymbol)
                {
                    result.Start = (Symbol)destSymbol;
                }
            }

            var prodStack = new Stack<Tuple<int,Production>>(
                                from p in grammar.Productions
                                where !p.IsAugmented
                                select Tuple.Create(0, converter.Convert(p)));
            
            while (prodStack.Count != 0)
            {
                var item = prodStack.Pop();
                var pos  = item.Item1;
                var prod = item.Item2;
                if (pos == prod.Size)
                {
                    if (!symbolsToInline.Contains(prod.Outcome))
                    {
                        result.Productions.Add(prod);
                    }

                    continue;
                }

                var symbol = prod.Pattern[pos];
                if (!symbolsToInline.Contains(symbol))
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
                        p => p.Pattern.All(s => s.IsTerminal)));
        }

        public IEnumerable<Production> Inline(
            Production parentProduction,
            int        inlinePosition)
        {
            var inlinedSymbol = grammar.Symbols[parentProduction.PatternTokens[inlinePosition]];
            var oldPattern    = parentProduction.Pattern;

            foreach (var inlinedProd in inlinedSymbol.Productions)
            {
                var newPattern = new Symbol[oldPattern.Length - 1 + inlinedProd.Pattern.Length];

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

                var newProduction = new Production(parentProduction.Outcome, newPattern);
                foreach (var platformToAction in parentProduction.PlatformToAction)
                {
                    Type platform = platformToAction.Key;
                    newProduction.PlatformToAction.Set(
                        platform, 
                        InlineAction(
                            platformToAction.Value,
                            inlinedProd.PlatformToAction.Get(platform),
                            inlinePosition));
                }

                yield return newProduction;
            }
        }

        private CompositeProductionAction InlineAction(
            ProductionAction parentAction,
            ProductionAction inlinedAction,
            int              inlinePosition)
        {
            Debug.Assert(parentAction != null);
            Debug.Assert(inlinedAction != null);

            CompositeProductionAction result;
            if (parentAction is CompositeProductionAction)
            {
                result = (CompositeProductionAction)parentAction.Clone();
            }
            else
            {
                result = new CompositeProductionAction();
                result.Subactions.Add((SimpleProductionAction)parentAction.Clone());
            }

            IEnumerable<SimpleProductionAction> inlinedSubactions;
            if (inlinedAction is CompositeProductionAction)
            {
                inlinedSubactions = ((CompositeProductionAction)inlinedAction).Subactions;
            }
            else
            {
                inlinedSubactions = new [] { (SimpleProductionAction)inlinedAction };
            }

            // Shift inlined actions and insert before all other subactions
            int insertIndex = 0;
            foreach (var subaction in inlinedSubactions)
            {
                var clone = new SimpleProductionAction(subaction.Offset + inlinePosition, subaction.ArgumentCount);
                result.Subactions.Insert(insertIndex, clone);
                ++insertIndex;
            }

            return result;
        }
    }
}
