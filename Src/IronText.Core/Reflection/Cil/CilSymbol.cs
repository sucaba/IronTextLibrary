using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Framework;
using IronText.Reflection;

namespace IronText.Reflection.Managed
{
    class CilSymbol
    {
        public CilSymbol()
        {
            this.Literals = new HashSet<string>();
        }

        public Symbol          Symbol     { get; set; }

        public Type            SymbolType  { get; set; }

        public HashSet<string> Literals   { get; private set; }

        public SymbolCategory  Categories { get; set; }

        public int Id
        { 
            get { return Symbol == null ? -1 : Symbol.Index; } 
        }

        public string Name
        {
            get
            {
                if (Literals.Count == 1)
                {
                    return CilSymbolNaming.GetLiteralName(Literals.First());
                }

                return CilSymbolNaming.GetTypeName(SymbolType);
            }
        }

        public override string ToString() { return Name; }
    }
}
