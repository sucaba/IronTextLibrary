using System;
using System.Linq.Expressions;

namespace IronText.Reflection
{
    [Serializable]
    public class SemanticFormula : IProductionSemanticElement
    {
        private IProductionSemanticScope scope;

        public SemanticFormula(
            SemanticVariable lhe,
            ISemanticValue[] arguments,
            LambdaExpression func)
        {
            this.Lhe        = lhe;
            this.Arguments = arguments;
        }

        public SemanticFormula(
            SemanticVariable  lhe,
            SemanticReference rhe)
        {
            this.Lhe       = lhe;
            this.Arguments = new[] { rhe };
            this.IsCopy    = true;
        }

        public SemanticFormula(
            SemanticVariable lhe,
            SemanticConstant rhe)
        {
            this.Lhe       = lhe;
            this.Arguments = new [] { rhe };
            this.IsCopy    = true;
        }

        public SemanticVariable Lhe        { get; private set; }

        public ISemanticValue[] Arguments { get; private set; }

        public bool IsCopy { get; private set; }

        public bool IsCalledOnReduce { get { return Lhe.Position < 0; } }

        void IProductionSemanticElement.Attach(IProductionSemanticScope scope)
        {
            this.scope = scope;
            ((IProductionSemanticElement)Lhe).Attach(scope);
            foreach (var arg in Arguments)
            {
                var needsScope = arg as IProductionSemanticElement;
                if (needsScope != null)
                {
                    needsScope.Attach(scope);
                }
            }
        }
    }
}