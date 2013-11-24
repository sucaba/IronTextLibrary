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

        public abstract TokenCategory Categories { get; set; }

        /// <summary>
        /// Determines whether symbol is terminal
        /// </summary>
        public virtual bool IsTerminal { get { return false; } set { } }

        public abstract Precedence Precedence { get; set; }

        public virtual bool IsAmbiguous { get { return false; } }

        public virtual ReferenceCollection<Production> Productions { get { return new ReferenceCollection<Production>(); } }
    }
}
