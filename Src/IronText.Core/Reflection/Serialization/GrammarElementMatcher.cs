using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Misc;

namespace IronText.Reflection
{
    class GrammarElementMatcher : ISymbolTextMatcher, IProductionTextMatcher
    {
        public bool MatchSymbol(Symbol symbol, string text)
        {
            return symbol.Name == text;
        }

        public bool MatchProduction(Production production, string text)
        {
            var sketch = ProductionSketch.Parse(text);
            return MatchProduction(production, sketch);
        }
        public bool MatchProduction(Production production, string outcome, IEnumerable<string> pattern)
        {
            bool result = MatchSymbol(production.Outcome, outcome)
                       && MatchComponents(production, pattern);

            return result;
        }

        public bool MatchProduction(Production production, ProductionSketch sketch)
        {
            if (sketch == null || sketch.Outcome != production.Outcome.Name)
            {
                return false;
            }

            bool result = MatchComponents(production, sketch.Components);
            return result;
        }

        private bool MatchComponents(Production production, IEnumerable<object> components)
        {
            bool result = production.ChildComponents.Length == components.Count()
                       && Enumerable.Zip(production.ChildComponents, components, MatchProductionComponent).All(s => s);
            return result;
        }

        private bool MatchProductionComponent(IProductionComponent component, object sketchComp)
        {
            Symbol     symbol;
            Production prod;
            switch (component.Match(out symbol, out prod))
            {
                case 0: return symbol.Name == (sketchComp as string); 
                case 1: return MatchProduction(prod, sketchComp as ProductionSketch);
                default:
                    throw new ArgumentException("component");
            }
        }
    }
}
