using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Misc;

namespace IronText.Reflection
{
    class GrammarElementMatcher 
        : ISymbolTextMatcher
        , IProductionTextMatcher
        , IInjectedActionParameterTextMatcher
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

        public bool Match(InjectedActionParameter p, string text)
        {
            int len = p.Name.Length;
            if (len + 1 != text.Length)
            {
                return false;
            }

            return text.Length != 0 
                && text[0] == '?' 
                && 0 == string.Compare(p.Name, 0, text, 1, len);
        }

        private bool MatchComponents(Production production, IEnumerable<object> components)
        {
            bool result = production.ChildComponents.Length == components.Count()
                       && Enumerable.Zip(production.ChildComponents, components, MatchComponent).All(s => s);
            return result;
        }

        private bool MatchComponent(IProductionComponent component, object sketchComp)
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
