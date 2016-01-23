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
            LambdaExpression    func)
        {
            this.Lhe        = lhe;
            this.ActualRefs = actualRefs;
        }

        public SemanticFormula(
            SemanticVariable  lhe,
            SemanticReference rhe)
        {
            this.Lhe        = lhe;
            this.ActualRefs = new[] { rhe };
            this.IsCopy     = true;
        }

        public SemanticVariable    Lhe        { get; private set; }

        public SemanticReference[] ActualRefs { get; private set; }

        public bool IsCopy { get; private set; }
    }
}