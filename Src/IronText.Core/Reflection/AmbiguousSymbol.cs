using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using IronText.Collections;
using IronText.Misc;

namespace IronText.Reflection
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

        public AmbiguousSymbol(Symbol main, Symbol[] symbols)
        {
            this.MainToken = main == null ? -1 : main.Index;
            this.Tokens    = new ReadOnlyCollection<int>((from s in symbols select s.Index).ToArray());
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

        public override SymbolCategory Categories
        {
            get { return SymbolCategory.None; }
            set { }
        }

        public override Precedence Precedence { get { return null; } set { } }

        public override bool IsAmbiguous { get { return true; } }

        public override ReferenceCollection<Production> Productions
        {
            get { throw new NotSupportedException(); }
        }

        protected override SymbolBase DoClone()
        {
            return new AmbiguousSymbol(MainToken, Tokens);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append("{");

            bool first = true;
            foreach (var token in Tokens)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    builder.Append("|");
                }

                builder.Append(Scope.Symbols[token]);
            }

            builder.Append("}");

            return builder.ToString();
        }

        protected override object DoGetIdentity()
        {
            return IdentityFactory.FromIntegers(Tokens);
        }
    }
}
