using IronText.Reflection;
using IronText.Reflection.Reporting;

namespace IronText.Extensibility
{
    internal class StackSemanticBinding : ISemanticBinding
    {
        /// <summary>
        /// ID of the parent state
        /// </summary>
        public int           StackState          { get; set; }

        /// <summary>
        /// Production which contains providing scope
        /// </summary>
        public Production    ProvidingProduction { get; set; }

        /// <summary>
        /// Tail relative position of the token with semantic scope instance in stack
        /// </summary>
        public int           StackLookback       { get; set; }

        /// <summary>
        /// Production which consumes semantic value
        /// </summary>
        public Production    ConsumingProduction { get; set; }

        public SemanticScope Scope               { get; set; }

        public SemanticRef   Reference           { get; set; }
    }
}
