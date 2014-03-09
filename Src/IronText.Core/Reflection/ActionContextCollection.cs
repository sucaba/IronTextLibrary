using IronText.Collections;

namespace IronText.Reflection
{
    public class ActionContextCollection : IndexedCollection<ActionContext,ISharedGrammarEntities>
    {
        public ActionContextCollection(ISharedGrammarEntities ebnfGrammar)
            : base(ebnfGrammar)
        {
        }

        public bool FindOrAdd(string name, out ActionContext output)
        {
            foreach (var item in this)
            {
                if (item.UniqueName == name)
                {
                    output = item;
                    return false;
                }
            }

            output = Add(new ActionContext(name));
            return true;
        }
    }
}
