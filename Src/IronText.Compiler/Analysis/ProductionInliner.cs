using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IronText.Reflection;

namespace IronText.Analysis
{
    public class ProductionInliner : IProductionInliner
    {
        private readonly Grammar grammar;

        public ProductionInliner(Grammar sourceGrammar)
        {
            this.grammar = sourceGrammar;
        }

        public Grammar Inline()
        {
            var result = new Grammar();

            IGrammarConverter converter = new IndexPreservingGrammarConverter(grammar, result);

            var symbolsToInline = new HashSet<Symbol>();

            foreach (var symbol in grammar.Symbols)
            {
                var det = symbol as Symbol;
                if (CanInline(det))
                {
                    symbolsToInline.Add(converter.Convert(det));
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
                throw new NotImplementedException("TODO");
#if false
                newProduction.Action = 
                    InlineAction(
                        parentProduction.Action,
                        inlinedProd.Action,
                        inlinePosition);
#endif

                yield return newProduction;
            }
        }

#if false
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
#endif
    }
}
