using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using IronText.Collections;

namespace IronText.Reflection
{
    public class Condition : IndexableObject<ISharedGrammarEntities>
    {
        public Condition(string name)
        {
            this.Name     = name;
            this.Matchers = new ReferenceCollection<Matcher>();
            this.Joint    = new Joint();
        }

        public string Name { get; private set; }

        public ReferenceCollection<Matcher> Matchers { get; private set; }

        public Joint Joint { get; private set; }

        protected override void DoAttached()
        {
            base.DoAttached();
            Matchers.Owner = Context.Matchers;
        }

        protected override void DoDetaching()
        {
            Matchers.Owner = null;
            base.DoDetaching();
        }
    }
}
