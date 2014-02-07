using IronText.Reflection;

namespace IronText.Extensibility
{
    internal class ProductionContextLink
    {
        /// <summary>
        /// ID of the parent state
        /// </summary>
        public int               ParentState { get; set; }

        /// <summary>
        /// Tail relative position of the context token in stack
        /// </summary>
        public int               ContextTokenLookback { get; set; }

        public Symbol            Provider { get; set; }

        public ProductionContext Consumer { get; set; }
    }
}
