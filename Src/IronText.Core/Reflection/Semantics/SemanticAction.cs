using IronText.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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


        class CopySemanticAction : SemanticAction
        {
            public CopySemanticAction(Production production, ISymbolProperty from, ISymbolProperty to)
                : base(production)
            {
                this.From = from;
                this.To   = to;
            }

            public ISymbolProperty From { get; private set; }

            public ISymbolProperty To   { get; private set; }
        }
    }
}
