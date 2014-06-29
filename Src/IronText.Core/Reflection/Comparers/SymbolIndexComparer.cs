using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Reflection
{
    internal class SymbolIndexComparer : IEqualityComparer<SymbolBase>, IEqualityComparer
    {
        public static readonly SymbolIndexComparer Instance = new SymbolIndexComparer();

        public bool Equals(SymbolBase x, SymbolBase y)
        {
            return (x == y) || (x != null && y != null && x.Index == y.Index);
        }

        public int GetHashCode(SymbolBase obj)
        {
            return obj == null ? -1 : obj.Index;
        }

        public bool Equals(object x, object y)
        {
            return Equals(x as SymbolBase, y as SymbolBase);
        }

        public int GetHashCode(object obj)
        {
            return GetHashCode(obj as SymbolBase);
        }
    }
}
