using IronText.Collections;
using IronText.Misc;
using IronText.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Semantics
{
    internal class InheritedPropertyEC
        : List<int>
        , IIndexable<IGrammarScope>
        , IIndexableImpl<IGrammarScope>
        , IHasIdentity
    {
        private IGrammarScope scope;


        public int  Index           { get; private set; }

        public bool IsDetached      { get; private set; }

        public bool IsSoftRemoved   { get; private set; }

        object IHasIdentity.Identity { get { return this; } }

        public void AssignIndex(int index)
        {
            this.Index = index;
        }

        public void Attached(IGrammarScope scope)
        {
            this.scope = scope;
        }

        public void Detaching(IGrammarScope scope)
        {
            this.scope = null;
        }

        public void MarkSoftRemoved()
        {
            IsSoftRemoved = true;
        }
    }
}
