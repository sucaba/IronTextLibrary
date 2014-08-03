using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Collections;
using IronText.Reflection.Utils;

namespace IronText.Reflection
{
    [Serializable]
    public class ProductionCollection : IndexedCollection<Production, IGrammarScope>
    {
        public ProductionCollection(IGrammarScope scope)
            : base(scope)
        {
        }

        public Production Add(string producitonText)
        {
            var sketch = ProductionSketch.Parse(producitonText);

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

        public Production Add(string outcome, IEnumerable<string> pattern, SemanticRef contextRef = null)
        {
            return Add(
                Scope.Symbols.ByName(outcome, createMissing: true),
                pattern.Select(name => Scope.Symbols.ByName(name, createMissing: true)),
                contextRef);
        }

        public Production Add(Symbol outcome, IEnumerable<Symbol> pattern, SemanticRef contextRef = null)
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
            foreach (var prod in this)
            {
                if (prod.EqualTo(sketch))
                {
                    return prod;
                }
            }

            return null;
        }
    }
}
