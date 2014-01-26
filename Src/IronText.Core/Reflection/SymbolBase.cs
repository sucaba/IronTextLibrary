using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using IronText.Collections;

namespace IronText.Reflection
{
    [DebuggerDisplay("Name = {Name}")]
    public abstract class SymbolBase : IndexableObject<IEbnfContext>, ICloneable
    {
        /// <summary>
        /// Display name
        /// </summary>
        public string Name { get; protected set; }

        public bool IsPredefined { get { return 0 <= Index && Index < EbnfGrammar.PredefinedSymbolCount; } }

        public abstract SymbolCategory Categories { get; set; }

        /// <summary>
        /// Determines whether symbol is terminal
        /// </summary>
        public virtual bool IsTerminal { get { return false; } }

        public virtual bool IsAmbiguous { get { return false; } }

        public abstract Precedence Precedence { get; set; }

        public abstract ReferenceCollection<Production> Productions { get; }

        public object Clone()
        {
            return DoClone();
        }

        protected abstract SymbolBase DoClone();

        public override string ToString()
        {
            return Name;
        }
    }
}
