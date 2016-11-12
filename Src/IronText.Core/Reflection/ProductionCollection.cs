﻿using IronText.DI;
using System;
using System.Collections.Generic;
using System.Linq;

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
            var sketch = ProductionSketch.Parse(text);

            var resolver = DR.Get<IProductionResolver>();
            var result = resolver.Create(sketch);

            return result;
        }

        public Production Add(string outcome, IEnumerable<string> pattern)
        {
            var sketch = new ProductionSketch(outcome, pattern);

            var resolver = DR.Get<IProductionResolver>();
            var result = resolver.Create(sketch);
            return result;
        }

        public Production Add(Symbol outcome, Symbol[] pattern, SemanticRef contextRef = null)
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
            var resolver = DR.Get<IProductionResolver>();

            var result = resolver.Find(sketch);
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
                    && Enumerable.SequenceEqual(prod.Input, pattern))
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
