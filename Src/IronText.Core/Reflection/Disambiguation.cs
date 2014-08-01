
namespace IronText.Reflection
{
    /// <summary>
    /// Describes disambiguation approach for lexical ambiguity (aka Schrodinger's token)
    /// </summary>
    public enum Disambiguation
    {
        /// <summary>
        /// Proper alternative is chosen depending on a parser state
        /// </summary>
        Contextual,

        /// <summary>
        /// Alternative has priority over other non-exclusive alternatives.
        /// </summary>
        /// <remarks>
        /// It is error to have more than one exclusive alternative within some ambiguity
        /// </remarks>
        Exclusive,
    }
}
