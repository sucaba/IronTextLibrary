using System;

namespace IronText.Extensibility
{
    public struct LocalParseContext
    {
        /// <summary>
        /// ID of the parent state
        /// </summary>
        public int  ParentState;

        /// <summary>
        /// Context token type <seealso cref="DemandAttribute"/>
        /// </summary>
        public Type ContextTokenType;

        /// <summary>
        /// Tail relative position of the context item in stack
        /// </summary>
        public int  ContextLookbackPos;

        // Child context type accessible from the context token
        public Type ChildType;
    }
}
