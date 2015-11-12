using System.Collections;
using System.Collections.Generic;

namespace IronText.Reflection
{
    internal class SymbolIndexComparer : IEqualityComparer<Symbol>, IEqualityComparer
    {
        public static readonly SymbolIndexComparer Instance = new SymbolIndexComparer();

        public bool Equals(Symbol x, Symbol y)
        {
            return (x == y) || (x != null && y != null && x.Index == y.Index);
        }

        public int GetHashCode(Symbol obj)
        {
            return obj == null ? -1 : obj.Index;
        }

        public new bool Equals(object x, object y)
        {
            return Equals(x as Symbol, y as Symbol);
        }

        public int GetHashCode(object obj)
        {
            return GetHashCode(obj as Symbol);
        }
    }
}
