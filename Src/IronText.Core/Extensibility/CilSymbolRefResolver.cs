using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Framework.Reflection;

namespace IronText.Extensibility
{
    internal class CilSymbolRefResolver : ITokenRefResolver
    {
        private readonly Dictionary<CilSymbolRef, CilSymbolDef> ref2def = new Dictionary<CilSymbolRef, CilSymbolDef>();
        
        public CilSymbolRefResolver()
        {
        }

        public IEnumerable<CilSymbolDef> Definitions
        {
            get { return ref2def.Values.Distinct(); }
        }

        public CilSymbolDef Resolve(CilSymbolRef tid)
        {
            CilSymbolDef result;
            if (tid == null || !ref2def.TryGetValue(tid, out result))
            {
                result = null;
            }

            return result;
        }

        public bool Contains(CilSymbolRef tokenRef)
        {
            return ref2def.ContainsKey(tokenRef);
        }

        public int GetId(CilSymbolRef tid)
        {
            var symbol = GetSymbol(tid);
            return symbol == null ? -1 : symbol.Index;
        }

        public Symbol GetSymbol(CilSymbolRef tid)
        {
            CilSymbolDef def = Resolve(tid);
            if (def == null)
            {
                return null;
            }

            return def.Symbol;
        }

        public void SetId(CilSymbolRef tid, Symbol symbol)
        {
            CilSymbolDef def = Ensure(tid);
            def.Symbol = symbol;
        }

        private CilSymbolDef Ensure(CilSymbolRef tid)
        {
            CilSymbolDef def = Resolve(tid);
            if (def == null)
            {
                def = new CilSymbolDef();
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

        public void Link(params CilSymbolRef[] tokenRefs)
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

        private void Link2(CilSymbolRef x, CilSymbolRef y)
        {
            var xDef = Resolve(x);
            var yDef = Resolve(y);

            if (xDef == null && yDef == null)
            {
                var def = new CilSymbolDef();
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

        private void MergeDefs(CilSymbolDef xDef, CilSymbolDef yDef)
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

        private void AttachRef(CilSymbolDef def, CilSymbolRef tokenRef)
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
