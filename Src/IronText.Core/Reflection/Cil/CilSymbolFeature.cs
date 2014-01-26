using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Reflection.Managed
{
    public struct CilSymbolFeature<T>
    {
        private readonly CilSymbolRef _symbol;
        private readonly T            _value;

        public CilSymbolFeature(CilSymbolRef symbol, T value)
        {
            this._symbol = symbol;
            this._value = value;
        }

        public CilSymbolRef Symbol { get { return _symbol; } }

        public T            Value { get { return _value; } }
    }
}
