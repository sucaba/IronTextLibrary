using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using IronText.Framework.Collections;

namespace IronText.Framework.Reflection
{
    /// <summary>
    /// Ambiguous token. Represents unresolved alternative betwen 2 or more tokens.
    /// </summary>
    /// <remarks>
    /// Alternative can be resolved in a deterministic way when current parser state 
    /// allows only single alternative or by a forking extended-GLR parsing paths.
    /// </remarks>
    public sealed class AmbiguousSymbol : SymbolBase
    {
        public AmbiguousSymbol(int mainToken, IEnumerable<int> tokens)
        {
            this.MainToken = mainToken;
            this.Tokens    = new ReadOnlyCollection<int>(tokens.ToArray());
        }

        /// <summary>
        /// The most probable mandatory token ID 
        /// or <c>-1</c> if there is no main token.
        /// </summary>
        public int MainToken { get; private set; }

        /// <summary>
        /// Token alternatives
        /// </summary>
        public ReadOnlyCollection<int> Tokens { get; private set; }

        public override TokenCategory Categories
        {
            get { return TokenCategory.None; }
            set { }
        }

        public override Precedence Precedence { get { return null; } set { } }

        public override bool IsAmbiguous { get { return true; } }

        public override ReferenceCollection<Production> Productions
        {
            get { throw new NotSupportedException(); }
        }
    }
}
