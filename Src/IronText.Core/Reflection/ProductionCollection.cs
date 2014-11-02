using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Collections;

namespace IronText.Reflection
{
    [Serializable]
    public class ProductionCollection : GrammarEntityCollection<Production, IGrammarScope>, IAddOnlyCollection<Production>
    {
        public ProductionCollection(IGrammarScope scope)
            : base(scope)
        {
        }

        public Production Add(string text)
        {
            var resolver = DR.Resolve<IProductionNameResolver>();
            var result = resolver.Resolve(text, createMissing: true);
            return result;
        }

        public Production Add(string outcome, IEnumerable<string> pattern)
        {
            var resolver = DR.Resolve<IProductionNameResolver>();
            var result = resolver.Resolve(outcome, pattern, createMissing: true);
            return result;
        }

        public Production Add(Symbol outcome, IEnumerable<IProductionComponent> pattern, SemanticRef contextRef = null)
        {
            var result = new Production(outcome, pattern, contextRef);
            return Add(result);
        }

        public Production Find(string text)
        {
            var sketch = ProductionSketch.Parse(text);
            return Find(sketch);
        }

        public Production Find(string outcome, string[] pattern)
        {
            var sketch = new ProductionSketch(outcome, pattern);
            return Find(sketch);
        }
        
        private Production Find(ProductionSketch sketch)
        {
            var resolver = DR.Resolve<IProductionNameResolver>();

            var result = resolver.Resolve(sketch, createMissing: false);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outcome"></param>
        /// <param name="pattern"></param>
        /// <returns>Rule ID or -1 if there is no such rule</returns>
        public Production Find(Symbol outcome, Symbol[] pattern)
        {
            foreach (var prod in this)
            {
                if (prod.Outcome == outcome
                    && prod.Input.Length == pattern.Length
                    && Enumerable.SequenceEqual(prod.Input, pattern)
                    && !prod.IsHidden)
                {
                    return prod;
                }
            }

            return null;
        }

        void IAddOnlyCollection<Production>.Add(Production item)
        {
            base.Add(item);
        }
    }
}
