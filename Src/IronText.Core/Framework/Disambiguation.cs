using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Framework
{
    /// <summary>
    /// Describes disambiguation approach for an alternative entry
    /// within some ambiguity
    /// </summary>
    public enum Disambiguation
    {
        Undefined = 0,

        /// <summary>
        /// Alternative has priority over other non-exclusive alternatives.
        /// </summary>
        /// <remarks>
        /// It is error to have more than one exclusive alternative within ambiguity
        /// </remarks>
        Exclusive,

        /// <summary>
        /// Proper alternative is chosen depending on a context
        /// </summary>
        Contextual
    }
}
