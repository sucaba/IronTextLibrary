using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Framework;
using IronText.Framework.Reflection;

namespace IronText.Extensibility
{
    class TokenDef
    {
        public int Id { get { return Symbol == null ? -1 : Symbol.Index; } }
        public Symbol Symbol;
        public readonly HashSet<string> Literals = new HashSet<string>();
        public SymbolCategory Categories;

        public Type TokenType { get; set; }

        public string Name
        {
            get
            {
                if (Literals.Count == 1)
                {
                    return TokenNaming.GetLiteralName(Literals.First());
                }

                return TokenNaming.GetTypeName(TokenType);
            }
        }

        public override string ToString() { return Name; }
    }
}
