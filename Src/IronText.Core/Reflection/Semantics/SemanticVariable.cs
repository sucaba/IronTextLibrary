using System;
using IronText.Runtime.Semantics;

namespace IronText.Reflection
{
    [Serializable]
    public class SemanticVariable : IProductionSemanticElement, ISemanticVariable
    {
        public const int OutcomePosition = -1;
        private IProductionSemanticScope scope;

        /// <summary>
        /// </summary>
        /// <param name="position">
        /// 0 is always reserved for an inherited attribute (outcome or left-side attribute),
        /// positive indexes are used to resolve name duplicates</param>
        /// <param name="name"></param>
        /// negative indexes means first attribute with the name.
        /// <param name="type"></param>
        public SemanticVariable(string name, int position = OutcomePosition)
        {
            this.Name     = name;
            this.Position = position;
        }

        /// <summary>
        /// Main synthesized result.
        /// </summary>
        /// <param name="type"></param>
        public SemanticVariable()
            : this(SynthesizedPropertyNames.Main, OutcomePosition)
        {
        }

        public string Name     { get; private set; }

        public int    Position { get; private set; }

        private bool IsInherited => Position >= 0;

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

        public IRuntimeVariable ToRuntime(int currentPosition)
        {
            Symbol symbol = ResolveSymbol();
            ISymbolProperty property = scope.ResolveProperty(symbol, Name, IsInherited);

            int offset = currentPosition - Position;
            return property.ToRuntimeVariable(offset);
        }
    }
}