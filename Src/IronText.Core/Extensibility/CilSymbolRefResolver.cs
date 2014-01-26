using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Framework.Reflection;

namespace IronText.Extensibility
{
    internal class CilSymbolRefResolver : ICilSymbolResolver
    {
        private readonly Dictionary<object, CilSymbolDef> ref2def = new Dictionary<object, CilSymbolDef>();
        
        public CilSymbolRefResolver()
        {
        }

        public IEnumerable<CilSymbolDef> Definitions
        {
            get { return ref2def.Values.Distinct(); }
        }

        public CilSymbolDef Resolve(CilSymbolRef tid)
        {
            if (tid == null)
            {
                return null;
            }

            var literalDef   = ResolveLiteral(tid.LiteralText);
            var tokenTypeDef = ResolveTokenType(tid.TokenType);

            if (literalDef != null && tokenTypeDef != null && literalDef != tokenTypeDef)
            {
                throw new InvalidOperationException("Unable to resolve conflicting token reference: " + tid);
            }

            return literalDef ?? tokenTypeDef;
        }

        private CilSymbolDef ResolveLiteral(string literal)
        {
            if (literal == null)
            {
                return null;
            }

            CilSymbolDef result;
            ref2def.TryGetValue(literal, out result);
            return result;
        }

        private CilSymbolDef ResolveTokenType(Type tokenType)
        {
            if (tokenType == null)
            {
                return null;
            }

            CilSymbolDef result;
            ref2def.TryGetValue(tokenType, out result);
            return result;
        }

        public bool Contains(CilSymbolRef tokenRef)
        {
            return tokenRef != null
                && (ResolveLiteral(tokenRef.LiteralText) != null
                    || ResolveTokenType(tokenRef.TokenType) != null);
        }

        public int GetId(CilSymbolRef tid)
        {
            var symbol = GetSymbol(tid);
            return symbol == null ? -1 : symbol.Index;
        }

        public Symbol GetSymbol(CilSymbolRef tid)
        {
            CilSymbolDef def = Resolve(tid);
            return def == null ? null : def.Symbol;
        }

        public void SetId(CilSymbolRef tid, Symbol symbol)
        {
            CilSymbolDef def = Ensure(tid);
            def.Symbol = symbol;
        }

        private CilSymbolDef Ensure(CilSymbolRef tid)
        {
            CilSymbolDef literalDef   = ResolveLiteral(tid.LiteralText);
            CilSymbolDef tokenTypeDef = ResolveTokenType(tid.TokenType);
            CilSymbolDef def          = MergeDefs(literalDef, tokenTypeDef);

            if (def == null)
            {
                 def = new CilSymbolDef();
            }
            else if (tid.TokenType != null && def.TokenType != null && def.TokenType != tid.TokenType)
            {
                throw new InvalidOperationException("Incompatible symbol constraints.");
            }

            // Add token to a defintion
            if (tid.TokenType != null)
            {
                def.TokenType = tid.TokenType;
            }

            if (tid.IsLiteral)
            {
                def.Literals.Add(tid.LiteralText);
            }

            // Update index
            foreach (var literal in def.Literals)
            {
                ref2def[literal] = def;
            }

            if (def.TokenType != null)
            {
                ref2def[def.TokenType] = def;
            }

            return def;
        }

        public void Link(CilSymbolRef first)
        {
            Ensure(first);
        }

        private CilSymbolDef MergeDefs(CilSymbolDef xDef, CilSymbolDef yDef)
        {
            if (xDef == null)
            {
                return yDef;
            }

            if (yDef == null)
            {
                return xDef;
            }

            if (xDef == yDef)
            {
                return xDef;
            }

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

            return xDef;
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
