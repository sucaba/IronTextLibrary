using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Reflection
{
    /// <summary>
    /// Describes disambiguation approach for lexical ambiguity (aka Schrodinger's token)
    /// </summary>
    public enum Disambiguation
    {
        /// <summary>
        /// Default value which should not be used in practice
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// Alternative has priority over other non-exclusive alternatives.
        /// </summary>
        /// <remarks>
        /// It is error to have more than one exclusive alternative within some ambiguity
        /// </remarks>
        Exclusive,

        /// <summary>
        /// Proper alternative is chosen depending on a parser state
        /// </summary>
        Contextual
    }
}
