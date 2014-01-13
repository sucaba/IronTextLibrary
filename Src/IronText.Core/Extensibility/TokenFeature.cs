using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Extensibility
{
    public struct TokenFeature<T>
    {
        private readonly TokenRef _token;
        private readonly T        _value;

        public TokenFeature(TokenRef token, T value)
        {
            this._token = token;
            this._value = value;
        }

        public TokenRef Token { get { return _token; } }

        public T        Value { get { return _value; } }
    }
}
