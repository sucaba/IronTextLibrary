using System;
using IronText.Collections;

namespace IronText.Extensibility
{
    internal class ProductionContextLink
    {
        public ProductionContextLink()
        {
            this.Joint = new Joint();
        }

        /// <summary>
        /// ID of the parent state
        /// </summary>
        public int  ParentState;

        /// <summary>
        /// Tail relative position of the context token in stack
        /// </summary>
        public int  ContextTokenLookback;

        public Joint Joint { get; private set; }
    }
}
