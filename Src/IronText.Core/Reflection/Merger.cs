using IronText.Collections;

namespace IronText.Reflection
{
    public class Merger : IndexableObject<ISharedGrammarEntities>
    {
        public Merger(Symbol symbol)
        {
            this.Symbol = symbol;
            this.Joint = new Joint();
        }

        public Symbol Symbol { get; private set; }

        public Joint Joint { get; private set; }
    }
}
