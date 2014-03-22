using IronText.Collections;

namespace IronText.Reflection
{
    public class SemanticContextCollection : IndexedCollection<SemanticContext,ISharedGrammarEntities>
    {
        public SemanticContextCollection(ISharedGrammarEntities ebnfGrammar)
            : base(ebnfGrammar)
        {
        }

        public bool FindOrAdd(string name, out SemanticContext output)
        {
            foreach (var item in this)
            {
                if (item.UniqueName == name)
                {
                    output = item;
                    return false;
                }
            }

            output = Add(new SemanticContext(name));
            return true;
        }
    }
}
