using System.Collections.Generic;
using System.Linq;

namespace IronText.Reflection
{
    class ProductionNameResolver : IProductionNameResolver
    {
        private readonly INameResolver<Symbol>          symbolNameResolver;
        private readonly IAddOnlyCollection<Production> collection;
        private readonly IProductionTextMatcher         matcher;

        public ProductionNameResolver(
            INameResolver<Symbol>   symbolNameResolver,
            IAddOnlyCollection<Production> productions,
            IProductionTextMatcher  textMatcher)
        {
            this.symbolNameResolver = symbolNameResolver;
            this.collection         = productions;
            this.matcher            = textMatcher;
        }

        public Production Resolve(string text, bool createMissing)
        {
            var sketch = ProductionSketch.Parse(text);
            return Resolve(sketch, createMissing);
        }

        public Production Resolve(string outcome, IEnumerable<string> components, bool createMissing)
        {
            var sketch = new ProductionSketch(outcome, components);
            return Resolve(sketch, createMissing);
        }

        public Production Resolve(ProductionSketch sketch, bool createMissing)
        {
            Production result = collection.FirstOrDefault(p => matcher.Match(p, sketch));
            if (result == null && createMissing)
            {
                result = CreateProduction(sketch);
                collection.Add(result);
            }

            return result;
        }

        private Symbol ResolveSymbol(string name)
        {
            return symbolNameResolver.Resolve(name, createMissing: true);
        }

        private Production CreateProduction(ProductionSketch sketch)
        {
            Symbol outcomeSymbol = ResolveSymbol(sketch.Outcome);
            var components = sketch.Components.Select(CreateComponent).ToArray();

            var result = new Production(outcomeSymbol, components, null);
            return result;
        }

        private IProductionComponent CreateComponent(object sketchOrText)
        {
            string asString = sketchOrText as string;
            if (asString != null)
            {
                if (asString.Length != 0 && asString[0] == '?')
                {
                    return ResolveInjecteActionParameter(asString.Substring(1));
                }

                return ResolveSymbol(asString);
            }

            var result = CreateProduction((ProductionSketch)sketchOrText);
            return result;
        }

        private IProductionComponent ResolveInjecteActionParameter(string name)
        {
            return new InjectedActionParameter(name);
        }

        public Production Resolve(string outcome, IEnumerable<string> pattern)
        {
            var sketch = new ProductionSketch(outcome, pattern);
            Symbol outcomeSymbol = ResolveSymbol(outcome);

            var result = new Production(
                outcomeSymbol,
                pattern.Select(ResolveSymbol),
                null);

            return result;
        }
    }
}
