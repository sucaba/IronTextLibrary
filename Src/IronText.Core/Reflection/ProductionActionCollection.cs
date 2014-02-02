using IronText.Collections;

namespace IronText.Reflection
{
    public class ProductionActionCollection : IndexedCollection<Production, ISharedGrammarEntities>
    {
        public ProductionActionCollection(ISharedGrammarEntities context)
            : base(context)
        {
        }
    }
}
