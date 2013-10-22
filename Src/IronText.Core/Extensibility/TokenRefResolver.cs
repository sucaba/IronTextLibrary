using System;
using System.Collections.Generic;
using System.Linq;

namespace IronText.Extensibility
{
    internal class TokenRefResolver : ITokenRefResolver
    {
        private readonly Dictionary<TokenRef, TokenDef> ref2def = new Dictionary<TokenRef, TokenDef>();
        
        public TokenRefResolver()
        {
        }

        public IEnumerable<TokenDef> Definitions
        {
            get { return ref2def.Values.Distinct(); }
        }

        public TokenDef Resolve(TokenRef tid)
        {
            TokenDef result;
            if (tid == null || !ref2def.TryGetValue(tid, out result))
            {
                result = null;
            }

            return result;
        }

        public bool Contains(TokenRef tokenRef)
        {
            return ref2def.ContainsKey(tokenRef);
        }

        public int GetId(TokenRef tid)
        {
            TokenDef def = Resolve(tid);
            if (def == null)
            {
                return -1;
            }

            return def.Id;
        }

        public void SetId(TokenRef tid, int id)
        {
            TokenDef def = Ensure(tid);
            def.Id = id;
        }

        private TokenDef Ensure(TokenRef tid)
        {
            TokenDef def = Resolve(tid);
            if (def == null)
            {
                def = new TokenDef();
                if (tid.IsLiteral)
                {
                    def.Literals.Add(tid.LiteralText);
                }
                else
                {
                    def.TokenType = tid.TokenType;
                }

                ref2def[tid] = def;
            }

            return def;
        }

        public void Link(params TokenRef[] tokenRefs)
        {
            if (tokenRefs.Length == 0)
            {
                return;
            }

            var first = tokenRefs[0];

            if (tokenRefs.Length == 1)
            {
                Ensure(first);
                return;
            }

            for (int i = 1; i != tokenRefs.Length; ++i)
            {
                Link2(first, tokenRefs[i]);
            }
        }

        private void Link2(TokenRef x, TokenRef y)
        {
            var xDef = Resolve(x);
            var yDef = Resolve(y);

            if (xDef == null && yDef == null)
            {
                var def = new TokenDef();
                AttachRef(def, x);
                AttachRef(def, y);
            }
            else if (xDef == null)
            {
                AttachRef(yDef, x);
            }
            else if (yDef == null)
            {
                AttachRef(xDef, y);
            }
            else
            {
                ref2def[y] = xDef;
                MergeDefs(xDef, yDef);
            }
        }

        private void MergeDefs(TokenDef xDef, TokenDef yDef)
        {
            if (xDef.TokenType != null 
                && yDef.TokenType != null
                && xDef.TokenType != yDef.TokenType)
            {
                var msg = string.Format(
                    "Internal error: attemt to identify single token by two types: '{0}' and '{1}'",
                    xDef.TokenType,
                    yDef.TokenType);
                throw new InvalidOperationException(msg);
            }

            if (xDef.TokenType == null)
            {
                xDef.TokenType = yDef.TokenType;
            }

            xDef.Literals.UnionWith(yDef.Literals);
        }

        private void AttachRef(TokenDef def, TokenRef tokenRef)
        {
            ref2def[tokenRef] = def;

            if (tokenRef.IsLiteral)
            {
                def.Literals.Add(tokenRef.LiteralText);
            }
            else
            {
                if (def.TokenType != null && def.TokenType != tokenRef.TokenType)
                {
                    var msg = string.Format(
                        "Internal error: attemt to identify single token by two types: '{0}' and '{1}'",
                        def.TokenType,
                        tokenRef.TokenType);
                    throw new InvalidOperationException(msg);
                }

                def.TokenType = tokenRef.TokenType;
            }
        }
    }
}
