using IronText.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Reflection
{
    /// <summary>
    /// Definition of runtime property of particular symbol (AKA Synthesized Attribute).
    /// </summary>
    [Serializable]
    public class SymbolProperty : IndexableObject<IGrammarScope>, ISymbolProperty
    {
        public SymbolProperty(Symbol symbol, string name)
        {
            this.Symbol = symbol;
            this.Name   = name;
        }

        public Symbol Symbol { get; private set; }

        public string Name   { get; private set; }
    }
}
