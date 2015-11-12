using IronText.Collections;
using IronText.Runtime;
using System;

namespace IronText.Reflection
{
    [Serializable]
    internal abstract class SemanticAction : IndexableObject<IGrammarScope>
    {
        public static SemanticAction MakeCopyOutToOut(Production production, ISymbolProperty from, ISymbolProperty to)
        {
            return new CopySemanticAction(production, from, to);
        }

        public SemanticAction(Production production)
        {
            this.Production = production;
        }

        public Production Production { get; private set; }

        public abstract void Invoke(ProductionActionArgs pargs);

        internal class CopySemanticAction : SemanticAction
        {
            public CopySemanticAction(Production production, ISymbolProperty from, ISymbolProperty to)
                : base(production)
            {
                this.From = from;
                this.To   = to;
            }

            public ISymbolProperty From { get; private set; }

            public ISymbolProperty To   { get; private set; }

            public override void Invoke(ProductionActionArgs pargs)
            {
                var production = Scope.Productions[pargs.ProductionIndex];
                var value = pargs.GetSynthesized(0, From.Name);
                //string toName = pargs.SetInherited(1, To.Name, value);
            }
        }
    }
}
