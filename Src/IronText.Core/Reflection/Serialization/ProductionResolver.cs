using System;
using System.Collections.Generic;
using System.Linq;

namespace IronText.Reflection
{
    class ProductionResolver : IProductionResolver
    {
        private readonly ISymbolResolver          symbolNameResolver;
        private readonly IAddOnlyCollection<Production> collection;
        private readonly IProductionTextMatcher         matcher;

        public ProductionResolver(
            ISymbolResolver         symbolNameResolver,
            IAddOnlyCollection<Production> productions,
            IProductionTextMatcher  textMatcher)
        {
            this.symbolNameResolver = symbolNameResolver;
            this.collection         = productions;
            this.matcher            = textMatcher;
        }

        public Production Find(ProductionSketch sketch)
        {
            Production result = collection.FirstOrDefault(p => matcher.Match(p, sketch));
            return result;
        }

        public Production Create(ProductionSketch sketch)
        {
            var result = CreateInstance(sketch);
            collection.Add(result);
            return result;
        }

        public Production Resolve(ProductionSketch sketch)
        {
            return Find(sketch) ?? Create(sketch);
        }

        private Symbol ResolveSymbol(string name)
        {
            return symbolNameResolver.Resolve(name);
        }

        private Production CreateInstance(ProductionSketch sketch)
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

            var result = CreateInstance((ProductionSketch)sketchOrText);
            return result;
        }

        private IProductionComponent ResolveInjecteActionParameter(string name)
        {
            return new InjectedActionParameter(name);
        }

        public Production Resolve(string outcome, string[] pattern)
        {
            var sketch = new ProductionSketch(outcome, pattern);
            Symbol outcomeSymbol = ResolveSymbol(outcome);

            return new Production(
                outcomeSymbol,
                Array.ConvertAll(pattern, ResolveSymbol));
        }
    }
}
