using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Framework.Reflection;

namespace IronText.Extensibility
{
    internal class CilSymbolRefResolver : ICilSymbolResolver
    {
        private readonly Dictionary<object, CilSymbol> ref2def = new Dictionary<object, CilSymbol>();
        
        public CilSymbolRefResolver()
        {
        }

        public IEnumerable<CilSymbol> Definitions
        {
            get { return ref2def.Values.Distinct(); }
        }

        public CilSymbol Resolve(CilSymbolRef tid)
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

        private CilSymbol ResolveLiteral(string literal)
        {
            if (literal == null)
            {
                return null;
            }

            CilSymbol result;
            ref2def.TryGetValue(literal, out result);
            return result;
        }

        private CilSymbol ResolveTokenType(Type tokenType)
        {
            if (tokenType == null)
            {
                return null;
            }

            CilSymbol result;
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
            CilSymbol def = Resolve(tid);
            return def == null ? null : def.Symbol;
        }

        public void SetId(CilSymbolRef tid, Symbol symbol)
        {
            CilSymbol def = Ensure(tid);
            def.Symbol = symbol;
        }

        private CilSymbol Ensure(CilSymbolRef tid)
        {
            CilSymbol literalDef   = ResolveLiteral(tid.LiteralText);
            CilSymbol tokenTypeDef = ResolveTokenType(tid.TokenType);
            CilSymbol def          = MergeDefs(literalDef, tokenTypeDef);

            if (def == null)
            {
                 def = new CilSymbol();
            }
            else if (tid.TokenType != null && def.SymbolType != null && def.SymbolType != tid.TokenType)
            {
                throw new InvalidOperationException("Incompatible symbol constraints.");
            }

            // Add token to a defintion
            if (tid.TokenType != null)
            {
                def.SymbolType = tid.TokenType;
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

            if (def.SymbolType != null)
            {
                ref2def[def.SymbolType] = def;
            }

            return def;
        }

        public void Link(CilSymbolRef first)
        {
            Ensure(first);
        }

        private CilSymbol MergeDefs(CilSymbol xDef, CilSymbol yDef)
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

            if (xDef.SymbolType != null 
                && yDef.SymbolType != null
                && xDef.SymbolType != yDef.SymbolType)
            {
                var msg = string.Format(
                    "Internal error: attemt to identify single token by two types: '{0}' and '{1}'",
                    xDef.SymbolType,
                    yDef.SymbolType);
                throw new InvalidOperationException(msg);
            }

            if (xDef.SymbolType == null)
            {
                xDef.SymbolType = yDef.SymbolType;
            }

            xDef.Literals.UnionWith(yDef.Literals);

            return xDef;
        }

        private void AttachRef(CilSymbol def, CilSymbolRef tokenRef)
        {
            ref2def[tokenRef] = def;

            if (tokenRef.IsLiteral)
            {
                def.Literals.Add(tokenRef.LiteralText);
            }
            else
            {
                if (def.SymbolType != null && def.SymbolType != tokenRef.TokenType)
                {
                    var msg = string.Format(
                        "Internal error: attemt to identify single token by two types: '{0}' and '{1}'",
                        def.SymbolType,
                        tokenRef.TokenType);
                    throw new InvalidOperationException(msg);
                }

                def.SymbolType = tokenRef.TokenType;
            }
        }
    }
}
