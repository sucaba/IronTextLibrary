
namespace IronText.Reflection
{
    /// <summary>
    /// Determines operator (terminal symbol) associativity.
    /// </summary>
    public enum Associativity : byte
    {
        /// <summary>
        /// Left associative operator.
        /// </summary>
        Left     = 1,

        /// <summary>
        /// Right associative operator.
        /// </summary>
        Right    = 2,

        /// <summary>
        /// It is a syntax error to find the same operator twice 'in a row'.
        /// </summary>
        None     = 0,
    }
}
