using System;
using IronText.Framework;

namespace IronText.Reflection.Managed
{
    [Serializable]
    public class CilSymbolRef : IEquatable<CilSymbolRef>
    {
        public static CilSymbolRef Create(Type tokenType, string literal)
        {
            return new CilSymbolRef(tokenType, literal);
        }
        
        public static CilSymbolRef Typed(Type tokenType)
        {
            if (tokenType == null)
            {
                throw new ArgumentException("tokenType");
            }

            return new CilSymbolRef(tokenType, null);
        }

        public static CilSymbolRef Literal(string literal)
        {
            if (string.IsNullOrEmpty(literal))
            {
                throw new ArgumentException("literal");
            }

            return new CilSymbolRef(null, literal);
        }

        public CilSymbolRef(Type tokenType, string literal)
        {
            this.TokenType   = tokenType;
            this.LiteralText = literal;
        }

        public bool Equals(CilSymbolRef other)
        {
            return other != null
                && (LiteralText == other.LiteralText 
                   && TokenType == other.TokenType);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CilSymbolRef);
        }

        public override int GetHashCode()
        {
            int result = 0;
            unchecked
            {
                if (TokenType != null)
                {
                    result += TokenType.GetHashCode();
                }

                if (LiteralText != null)
                {
                    result += LiteralText.GetHashCode();
                }
            }

            return result;
        }

        public override string ToString()
        {
            return string.Format("TID({0}, {1})", TokenType, LiteralText);
        }

        public bool IsLiteral
        {
            get { return LiteralText != null; }
        }

        /// <summary>
        /// Determines whether token is no-operation token.
        /// </summary>
        public bool IsNopToken
        {
            get { return TokenType == typeof(void); }
        }

        public Type TokenType { get; private set; }

        public string LiteralText { get; private set; }
        
    }
}
