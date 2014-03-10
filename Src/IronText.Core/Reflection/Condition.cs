using IronText.Collections;

namespace IronText.Reflection
{
    public class Condition : IndexableObject<ISharedGrammarEntities>
    {
        public Condition(string name)
        {
            this.Name     = name;
            this.ContextProvider = new ActionContextProvider();
            this.Matchers = new ReferenceCollection<Matcher>();
            this.Joint    = new Joint();
        }

        public string Name { get; private set; }

        /// <summary>
        /// Context provider for matchers in this condition.
        /// </summary>
        public ActionContextProvider ContextProvider { get; private set; }

        public ReferenceCollection<Matcher> Matchers { get; private set; }

        public Joint Joint { get; private set; }

        protected override void DoAttached()
        {
            base.DoAttached();
            Matchers.Owner = Scope.Matchers;
        }

        protected override void DoDetaching()
        {
            Matchers.Owner = null;
            base.DoDetaching();
        }
    }
}
