using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Reflection.Managed
{
    public struct CilSymbolFeature<T>
    {
        private readonly CilSymbolRef _symbolRef;
        private readonly T            _value;

        public CilSymbolFeature(CilSymbolRef symbolRef, T value)
        {
            this._symbolRef = symbolRef;
            this._value = value;
        }

        public CilSymbolRef SymbolRef { get { return _symbolRef; } }

        public T            Value     { get { return _value; } }
    }
}
