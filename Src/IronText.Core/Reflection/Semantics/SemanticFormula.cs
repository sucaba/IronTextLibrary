using System;
using System.Linq.Expressions;

namespace IronText.Reflection
{

    [Serializable]
    public class SemanticFormula
    {
        public SemanticFormula(
            SemanticVariable    lhe,
            SemanticReference[] actualRefs,
            LambdaExpression    formula)
        {
        }

        public SemanticFormula(
            SemanticVariable  lhe,
            SemanticReference rhe)
        {
        }

        public SemanticVariable    Lhe        { get; private set; }

        public SemanticReference[] ActualRefs { get; private set; }

        // Note: this should not be exposed because there maybe 
        // non-CLR ways to construct a formula.
        // internal LambdaExpression    Formula
    }
}