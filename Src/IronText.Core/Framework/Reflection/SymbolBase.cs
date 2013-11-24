using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using IronText.Framework.Collections;

namespace IronText.Framework.Reflection
{
    public abstract class SymbolBase : IndexableObject<IEbnfContext>
    {
        /// <summary>
        /// Display name
        /// </summary>
        public string Name { get; protected set; }

        public bool IsPredefined { get { return 0 <= Index && Index < EbnfGrammar.PredefinedTokenCount; } }

        public abstract TokenCategory Categories { get; set; }

        /// <summary>
        /// Determines whether symbol is terminal
        /// </summary>
        public virtual bool IsTerminal { get { return false; } }

        public virtual bool IsAmbiguous { get { return false; } }

        public abstract Precedence Precedence { get; set; }

        public abstract ReferenceCollection<Production> Productions { get; }
    }
}
