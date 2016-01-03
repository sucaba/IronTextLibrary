using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace IronText.Reflection
{
    public class ProductionSemantics : IEnumerable<SemanticFormula>
    {
        private readonly Production prod;

        public ProductionSemantics(Production prod)
        {
            this.prod = prod;
        }

        public void Add(SemanticFormula formula)
        {
            throw new NotImplementedException();
        }

        public void Add(
            SemanticVariable      lhe,
            SemanticReference     rhe)
        {
            throw new NotImplementedException();
        }

        public void Add<T>(
            SemanticVariable      lhe,
            SemanticReference[]   actualRefs,
            Expression<Func<T,T>> func)
        {
            throw new NotImplementedException();
        }

        public void Add<T1, T2, T3, T4, T>(
            SemanticVariable lhe,
            SemanticReference[] actualRefs,
            Expression<Func<T1, T2, T3, T4, T>> func)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator<SemanticFormula> IEnumerable<SemanticFormula>.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
