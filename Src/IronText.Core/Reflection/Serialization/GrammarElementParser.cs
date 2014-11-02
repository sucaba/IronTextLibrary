using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Reflection
{
    class GrammarElementParser : ISymbolParser, IProductionParser
    {
        private readonly IGrammarScope scope;

        public GrammarElementParser(IGrammarScope scope)
        {
            this.scope = scope;
        }

        public Symbol ParseSymbol(string outcome)
        {
            return scope.Symbols.ByName(outcome, createMissing: true);
        }

        public Production ParseProduction(string text)
        {
            var sketch = ProductionSketch.Parse(text);

            Production result = CreateProduction(sketch);
            return result;
        }

        public ProductionSketch BuildSketch(string text)
        {
            return ProductionSketch.Parse(text);
        }

        private Production CreateProduction(ProductionSketch sketch)
        {
            Symbol outcomeSymbol = ParseSymbol(sketch.Outcome);

            var result = new Production(
                outcomeSymbol,
                sketch.Components.Select(CreateComponent),
                null);

            return result;
        }

        private IProductionComponent CreateComponent(object sketchOrText)
        {
            string asString = sketchOrText as string;
            if (asString != null)
            {
                return ParseSymbol(asString);
            }

            var result = CreateProduction((ProductionSketch)sketchOrText);
            return result;
        }

        public Production ParseProduction(string outcome, IEnumerable<string> pattern)
        {
            var sketch = new ProductionSketch(outcome, pattern);
            Symbol outcomeSymbol = ParseSymbol(outcome);

            var result = new Production(
                outcomeSymbol,
                pattern.Select(ParseSymbol),
                null);

            return result;
        }
    }
}
