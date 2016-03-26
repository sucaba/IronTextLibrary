using System;

namespace IronText.Reflection
{
    [Serializable]
    public class SemanticReference : ISemanticValue, IProductionSemanticElement
    {
        public const int OutcomePosition = -1;
        private IProductionSemanticScope scope;

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="position">
        /// -1 is always reserved for an inherited attribute (outcome or left-side attribute),
        /// positive indexes are used to resolve name duplicates</param>
        /// negative indexes means first attribute with the name.
        public SemanticReference(string name, int position = OutcomePosition)
        {
            this.Name     = name;
            this.Position = position;
        }

        /// <summary>
        /// Reference to the right-side main synthesized attribute.
        /// </summary>
        /// <param name="position"></param>
        public SemanticReference(int position)
            : this(SynthesizedPropertyNames.Main, position)
        {
        }

        public string Name     { get; private set; }

        public int    Position { get; private set; }

        public Symbol ResolveSymbol()
        {
            if (scope == null)
            {
                throw new InvalidOperationException("Semantic element is not attached to production.");
            }

            var result = (Position < 0) ? scope.Outcome : scope.Input[Position];
            return result;
        }

        void IProductionSemanticElement.Attach(IProductionSemanticScope scope)
        {
            this.scope = scope;
        }
    }
}