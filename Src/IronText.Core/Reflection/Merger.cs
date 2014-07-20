using IronText.Collections;
using System;

namespace IronText.Reflection
{
    [Serializable]
    public class Merger : IndexableObject<IGrammarScope>
    {
        [NonSerialized]
        private readonly Joint _joint;

        public Merger(Symbol symbol)
        {
            this.Symbol = symbol;
            this._joint = new Joint();
        }

        public Symbol Symbol { get; private set; }

        public Joint Joint { get { return _joint; } }
    }
}
