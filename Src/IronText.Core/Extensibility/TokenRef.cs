using System;
using IronText.Framework;
using IronText.Runtime;

namespace IronText.Extensibility
{
    [Serializable]
    public class TokenRef : IEquatable<TokenRef>
    {
        private readonly int hash;

        public static TokenRef Typed(Type tokenType)
        {
            if (tokenType == null)
            {
                throw new ArgumentException("tokenType");
            }

            return new TokenRef(tokenType, null);
        }

        public static TokenRef Literal(string literal)
        {
            if (string.IsNullOrEmpty(literal))
            {
                throw new ArgumentException("literal");
            }

            return new TokenRef(null, literal);
        }

        public TokenRef(Type tokenType, string literal)
        {
            this.TokenType   = tokenType;
            this.LiteralText = literal;

            int hash = 0;
            unchecked
            {
                if (tokenType != null)
                {
                    hash += tokenType.GetHashCode();
                }

                if (literal != null)
                {
                    hash += literal.GetHashCode();
                }
            }

            this.hash = hash;
        }

        public bool Equals(TokenRef other)
        {
            return other != null
                && hash == other.hash
                && LiteralText == other.LiteralText
                && TokenType == other.TokenType;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TokenRef);
        }

        public override int GetHashCode() { return hash; }

        public override string ToString()
        {
            return string.Format("TID({0}, {1})", TokenType, LiteralText);
        }

        public string Name
        {
            get
            {
                if (IsLiteral)
                {
                    return TokenNaming.GetLiteralName(LiteralText);
                }

                return TokenNaming.GetTypeName(TokenType);
            }
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

        public bool IsExternal 
        { 
            get 
            {
                return TokenType != null
                    && typeof(IReceiver<Msg>).IsAssignableFrom(TokenType);
            } 
        }

        public Type TokenType { get; private set; }

        public string LiteralText { get; private set; }
        
    }
}
