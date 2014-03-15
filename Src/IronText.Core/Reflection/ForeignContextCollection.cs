using IronText.Collections;

namespace IronText.Reflection
{
    public class ForeignContextCollection : IndexedCollection<ForeignContext,ISharedGrammarEntities>
    {
        public ForeignContextCollection(ISharedGrammarEntities ebnfGrammar)
            : base(ebnfGrammar)
        {
        }

        public bool FindOrAdd(string name, out ForeignContext output)
        {
            foreach (var item in this)
            {
                if (item.UniqueName == name)
                {
                    output = item;
                    return false;
                }
            }

            output = Add(new ForeignContext(name));
            return true;
        }
    }
}
