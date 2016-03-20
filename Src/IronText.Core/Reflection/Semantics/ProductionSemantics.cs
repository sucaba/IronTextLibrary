using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace IronText.Reflection
{
    [Serializable]
    public class ProductionSemantics : IEnumerable<SemanticFormula>
    {
        private readonly Production prod;
        private List<SemanticFormula> formulas = new List<SemanticFormula>();

        public ProductionSemantics(Production prod)
        {
            this.prod = prod;
        }

        private void Add(SemanticFormula formula)
        {
            AddLhePropertyToGrammar(formula);

            formulas.Add(formula);
        }

        private void AddLhePropertyToGrammar(SemanticFormula formula)
        {
            var lhe = formula.Lhe;
            if (lhe.Position < 0)
            {
                var symbol = prod.Outcome;
                prod.Scope.SymbolProperties.FindOrAdd(symbol, lhe.Name);
            }
            else
            {
                var symbol = prod.Input[lhe.Position];
                prod.Scope.InheritedProperties.FindOrAdd(symbol, lhe.Name);
            }
        }

        public void Add(
            SemanticVariable      lhe,
            SemanticReference     rhe)
        {
            Add(new SemanticFormula(lhe, rhe));
        }

        public void Add<T>(
            SemanticVariable      lhe,
            SemanticReference[]   actualRefs,
            Expression<Func<T,T>> func)
        {
            Add(new SemanticFormula(lhe, actualRefs, func));
        }

        public void Add<T1, T2, T3, T4, T>(
            SemanticVariable lhe,
            SemanticReference[] actualRefs,
            Expression<Func<T1, T2, T3, T4, T>> func)
        {
            Add(new SemanticFormula(lhe, actualRefs, func));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return formulas.GetEnumerator();
        }

        IEnumerator<SemanticFormula> IEnumerable<SemanticFormula>.GetEnumerator()
        {
            return formulas.GetEnumerator();
        }
    }
}
