using IronText.Reflection;

namespace IronText.Extensibility
{
    internal abstract class SemanticBinding
    {
        public SemanticScope Scope     { get; set; }

        public SemanticRef   Reference { get; set; }
    }

    internal class StackSemanticBinding : SemanticBinding
    {
        /// <summary>
        /// ID of the parent state
        /// </summary>
        public int StackState    { get; set; }

        /// <summary>
        /// Tail relative position of the token with semantic scope instance in stack
        /// </summary>
        public int StackLookback { get; set; }
    }

    internal class InlinedSemanticBinding : SemanticBinding
    {
        /// <summary>
        /// Owning extended production
        /// </summary>
        public Production  OwningProduction    { get; set; }

        /// <summary>
        /// Breadth-first left-to-right position in component tree.
        /// </summary>
        public int         ProvidingPosition   { get; set; }

        /// <summary>
        /// Difference between indexes of providing and consuming components
        /// </summary>
        public int         Lookback            { get; set; }
    }
}
