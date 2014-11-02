using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Collections;

namespace IronText.Reflection
{
    [Serializable]
    public class ProductionCollection : GrammarEntityCollection<Production, IGrammarScope>
    {
        public ProductionCollection(IGrammarScope scope)
            : base(scope)
        {
        }

        public Production Add(string producitonText)
        {
            var parser = DR.Resolve<IProductionParser>();
            var sketch = parser.BuildSketch(producitonText);

            var result = Find(sketch);
            if (result == null)
            {
                result = Add(sketch);
            }

            return result;
        }

        private Production Add(ProductionSketch sketch)
        {
            throw new NotImplementedException();
        }

        public Production Add(string outcome, IEnumerable<string> pattern)
        {
            Production result;

            var matcher = DR.Resolve<IProductionTextMatcher>();
            result = this.FirstOrDefault(p => matcher.MatchProduction(p, outcome, pattern));
            if (result == null)
            {
                var parser = DR.Resolve<IProductionParser>();

                result = parser.ParseProduction(outcome, pattern);
                Add(result);
            }

            return result;
        }

        public Production Add(Symbol outcome, IEnumerable<IProductionComponent> pattern, SemanticRef contextRef = null)
        {
            var result = new Production(outcome, pattern, contextRef);
            return Add(result);
        }

        [Obsolete("Refactoring grammar indexing")]
        public Production Add(int outcome, IEnumerable<int> pattern, string contextName = null)
        {
            return Add(
                Scope.Symbols[outcome],
                pattern.Select(t => Scope.Symbols[t]),
                SemanticRef.ByName(contextName));
        }

        public Production Find(string productionText)
        {
            var sketch = ProductionSketch.Parse(productionText);
            return Find(sketch);
        }

        public Production Find(string outcome, string[] pattern)
        {
            var sketch = new ProductionSketch(outcome, pattern);
            return Find(sketch);
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
        
        private Production Find(ProductionSketch sketch)
        {
            var matcher = DR.Resolve<IProductionTextMatcher>();

            var result = this.FirstOrDefault(p => matcher.MatchProduction(p, sketch));
            return result;
        }
    }
}
