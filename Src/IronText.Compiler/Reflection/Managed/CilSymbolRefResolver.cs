using System;
using System.Collections.Generic;
using System.Linq;

namespace IronText.Reflection.Managed
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

        public CilSymbol Resolve(CilSymbolRef symbolRef)
        {
            if (symbolRef == null)
            {
                return null;
            }

            var literalDef   = ResolveLiteral(symbolRef.Literal);
            var tokenTypeDef = ResolveTokenType(symbolRef.Type);

            if (literalDef != null && tokenTypeDef != null && literalDef != tokenTypeDef)
            {
                throw new InvalidOperationException("Unable to resolve conflicting token reference: " + symbolRef);
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

        private CilSymbol ResolveTokenType(Type type)
        {
            if (type == null)
            {
                return null;
            }

            CilSymbol result;
            ref2def.TryGetValue(type, out result);
            return result;
        }

        public bool Contains(CilSymbolRef symbolRef)
        {
            return symbolRef != null
                && (ResolveLiteral(symbolRef.Literal) != null
                    || ResolveTokenType(symbolRef.Type) != null);
        }

        public Symbol GetSymbol(CilSymbolRef tid)
        {
            CilSymbol def = Resolve(tid);
            return def == null ? null : def.Symbol;
        }

        private CilSymbol Ensure(CilSymbolRef symbolRef)
        {
            CilSymbol literalDef = ResolveLiteral(symbolRef.Literal);
            CilSymbol typeDef    = ResolveTokenType(symbolRef.Type);
            CilSymbol def        = MergeDefs(literalDef, typeDef);

            if (def == null)
            {
                 def = new CilSymbol();
            }
            else if (symbolRef.Type != null && def.Type != null && def.Type != symbolRef.Type)
            {
                throw new InvalidOperationException("Incompatible symbol constraints.");
            }

            // Add token to a defintion
            if (symbolRef.Type != null)
            {
                def.Type = symbolRef.Type;
            }

            if (symbolRef.HasLiteral)
            {
                def.Literals.Add(symbolRef.Literal);
            }

            // Update index
            foreach (var literal in def.Literals)
            {
                ref2def[literal] = def;
            }

            if (def.Type != null)
            {
                ref2def[def.Type] = def;
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

            if (xDef.Type != null 
                && yDef.Type != null
                && xDef.Type != yDef.Type)
            {
                var msg = string.Format(
                    "Internal error: attemt to identify single token by two types: '{0}' and '{1}'",
                    xDef.Type,
                    yDef.Type);
                throw new InvalidOperationException(msg);
            }

            if (xDef.Type == null)
            {
                xDef.Type = yDef.Type;
            }

            xDef.Literals.UnionWith(yDef.Literals);

            return xDef;
        }
    }
}
