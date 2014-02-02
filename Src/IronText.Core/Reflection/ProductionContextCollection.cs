using IronText.Collections;

namespace IronText.Reflection
{
    public class ProductionContextCollection : IndexedCollection<ProductionContext,ISharedGrammarEntities>
    {
        public ProductionContextCollection(ISharedGrammarEntities ebnfGrammar)
            : base(ebnfGrammar)
        {
        }

        public bool FindOrAdd(string name, out ProductionContext output)
        {
            foreach (var item in this)
            {
                if (item.Name == name)
                {
                    output = item;
                    return false;
                }
            }

            output = Add(new ProductionContext(name));
            return true;
        }
    }
}
