using IronText.Collections;
using IronText.Runtime.Semantics;
using System;

namespace IronText.Reflection
{
    /// <summary>
    /// Represents 'Inherited Attribute' concept from ECLR papers.
    /// </summary>
    /// <remarks>
    /// Inherited means that symbol will recieve some value from environment 
    /// before symbol even appear in a stack.
    /// </remarks>
    [Serializable]
    public class InheritedProperty : IndexableObject<IGrammarScope>, ISymbolProperty
    {
        public InheritedProperty(Symbol symbol, string name)
        {
            this.Symbol = symbol;
            this.Name   = name;
        }

        public Symbol Symbol { get; private set; }

        public string Name   { get; private set; }

        public override string ToString()
        {
            return Symbol.Name + "." + Name;
        }

        public IRuntimeValue ToRuntimeValue(int offset)
        {
            var result = new InheritedRuntimeProperty(offset, Index);
            return result;
        }

        public IRuntimeVariable ToRuntimeVariable(int offset)
        {
            var result = new InheritedRuntimeProperty(offset, Index);
            return result;
        }
    }
}
