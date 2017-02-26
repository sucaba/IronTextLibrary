using IronText.Reflection;
using IronText.Reporting;

namespace IronText.Extensibility
{
    internal class StackSemanticBinding : ISemanticBinding
    {
        public string ProvidingProductionText => ProvidingProduction.DebugProductionText;

        public string ConsumingProductionText => ConsumingProduction.DebugProductionText;

        public string ReferenceName           => Reference.UniqueName;

        /// <summary>
        /// ID of the parent state
        /// </summary>
        internal int           StackState          { get; set; }

        /// <summary>
        /// Production which contains providing scope
        /// </summary>
        internal Production    ProvidingProduction { get; set; }

        /// <summary>
        /// Tail relative position of the token with semantic scope instance in stack
        /// </summary>
        internal int           StackLookback       { get; set; }

        /// <summary>
        /// Production which consumes semantic value
        /// </summary>
        internal Production    ConsumingProduction { get; set; }

        public SemanticScope   Scope               { get; set; }

        internal SemanticRef   Reference           { get; set; }
    }
}
