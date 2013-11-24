using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using IronText.Framework.Collections;

namespace IronText.Framework.Reflection
{
    public class Symbol : SymbolBase
    {
        public readonly ReferenceCollection<Production> productions;

        public Symbol(string name)
        {
            this.Name = name ?? EbnfGrammar.UnnamedTokenName;

            this.productions = new ReferenceCollection<Production>();
        }

        /// <summary>
        /// Categories token belongs to
        /// </summary>
        public override TokenCategory Categories { get; set; }

        /// <summary>
        /// Determines whether symbol is terminal
        /// </summary>
        public override bool IsTerminal { get; set; }

        /// <summary>
        /// Determines token-level precedence
        /// </summary>
        /// <remarks>
        /// If production has no associated precedence, it is calculated from
        /// the last terminal token in a production pattern.
        /// </remarks>
        public override Precedence Precedence { get; set; }

        public override ReferenceCollection<Production> Productions { get { return productions; } }

        public override bool Equals(object obj)
        {
            var casted = obj as Symbol;
            return casted != null
                && casted.Name == Name
                && casted.Categories == Categories
                && object.Equals(casted.Precedence, Precedence)
                ;
        }

        public override int GetHashCode()
        {
            int result = 0;
            unchecked
            {
                if (Name != null)
                {
                    result += Name.GetHashCode();
                }

                result += Categories.GetHashCode();
                if (Precedence != null)
                {
                    result += Precedence.GetHashCode();
                }
            }

            return result;
        }
    }
}
