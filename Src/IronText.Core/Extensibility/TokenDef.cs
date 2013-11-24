using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Framework;

namespace IronText.Extensibility
{
    class TokenDef
    {
        public int  Id = -1;
        public readonly HashSet<string> Literals = new HashSet<string>();
        public TokenCategory Categories;

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
