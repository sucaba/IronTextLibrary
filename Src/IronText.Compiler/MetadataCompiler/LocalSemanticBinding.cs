using IronText.Reflection;

namespace IronText.Extensibility
{
    internal class LocalSemanticBinding
    {
        /// <summary>
        /// ID of the parent state
        /// </summary>
        public int           StackState    { get; set; }

        /// <summary>
        /// Tail relative position of the token with semantic scope instance in stack
        /// </summary>
        public int           StackLookback { get; set; }

        public SemanticScope Locals        { get; set; }

        public SemanticRef   ConsumerRef   { get; set; }
    }
}
