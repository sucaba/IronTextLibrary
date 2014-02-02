using System;

namespace IronText.Reflection.Managed
{
    [Serializable]
    public class CilSymbolRef : IEquatable<CilSymbolRef>
    {
        public static CilSymbolRef Create(Type type, string literal)
        {
            return new CilSymbolRef(type, literal);
        }
        
        public static CilSymbolRef Create(Type type)
        {
            if (type == null)
            {
                throw new ArgumentException("type");
            }

            return new CilSymbolRef(type, null);
        }

        public static CilSymbolRef Create(string literal)
        {
            if (string.IsNullOrEmpty(literal))
            {
                throw new ArgumentException("literal");
            }

            return new CilSymbolRef(null, literal);
        }

        public CilSymbolRef(Type type, string literal)
        {
            this.Type    = type;
            this.Literal = literal;
        }

        public bool Equals(CilSymbolRef other)
        {
            return other != null
                && (Literal == other.Literal 
                   && Type == other.Type);
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
                if (Type != null)
                {
                    result += Type.GetHashCode();
                }

                if (Literal != null)
                {
                    result += Literal.GetHashCode();
                }
            }

            return result;
        }

        public override string ToString()
        {
            return string.Format("TID({0}, {1})", Type, Literal);
        }

        public bool HasLiteral { get { return Literal != null; } }

        public Type   Type     { get; private set; }

        public string Literal  { get; private set; }
    }
}
