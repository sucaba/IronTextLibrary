using IronText.Reflection;

namespace IronText.Extensibility
{
    internal class ProductionContextBinding
    {
        /// <summary>
        /// ID of the parent state
        /// </summary>
        public int               StackState            { get; set; }

        /// <summary>
        /// Tail relative position of the context token in stack
        /// </summary>
        public int               ProviderStackLookback { get; set; }

        public Symbol            Provider   	   	   { get; set; }

        public ProductionContext Consumer              { get; set; }
    }
}
