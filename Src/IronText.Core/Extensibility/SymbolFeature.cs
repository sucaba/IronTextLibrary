using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Extensibility
{
    public struct SymbolFeature<T>
    {
        private readonly CilSymbolRef _token;
        private readonly T        _value;

        public SymbolFeature(CilSymbolRef token, T value)
        {
            this._token = token;
            this._value = value;
        }

        public CilSymbolRef Token { get { return _token; } }

        public T            Value { get { return _value; } }
    }
}
