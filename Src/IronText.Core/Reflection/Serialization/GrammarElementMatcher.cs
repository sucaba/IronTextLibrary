using IronText.Misc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IronText.Reflection
{
    class GrammarElementMatcher 
        : ISymbolTextMatcher
        , IProductionTextMatcher
        , IInjectedActionParameterTextMatcher
    {
        public bool Match(Symbol symbol, string text)
        {
            return symbol.Name == text && (text.Length == 0 || text[0] != '?');
        }

        public bool Match(Production production, string text)
        {
            var sketch = ProductionSketch.Parse(text);
            return Match(production, sketch);
        }
        public bool Match(Production production, string outcome, IEnumerable<string> pattern)
        {
            bool result = Match(production.Outcome, outcome)
                       && MatchComponents(production, pattern);

            return result;
        }

        public bool Match(Production production, ProductionSketch sketch)
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
            InjectedActionParameter injectedParam;
            switch (component.Match(out symbol, out prod, out injectedParam))
            {
                case 0: return Match(symbol, sketchComp as string); 
                case 1: return Match(prod, sketchComp as ProductionSketch);
                case 2: return Match(injectedParam, sketchComp as string);
                default:
                    throw new ArgumentException("component");
            }
        }
    }
}
