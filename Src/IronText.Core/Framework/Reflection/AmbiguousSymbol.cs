using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace IronText.Framework.Reflection
{
    /// <summary>
    /// Ambiguous token
    /// </summary>
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
    }
}
