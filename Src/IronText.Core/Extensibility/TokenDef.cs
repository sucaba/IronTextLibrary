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
        private Type tokenType;

        public Type TokenType
        {
            get { return this.tokenType; }
            set 
            { 
                this.tokenType = value;
                if (typeof(IReceiver<Msg>).IsAssignableFrom(TokenType))
                {
                    this.Categories |= TokenCategory.External;
                }
            }
        }

        public bool IsExternal
        {
            get { return (Categories & TokenCategory.External) != 0; }
        }

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
