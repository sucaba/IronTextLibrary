using System.Collections.Generic;

namespace IronText.Runtime
{
    using Collections;
    using System.Diagnostics;
    using State = System.Int32;

    sealed class GssNode<T> : IStackLookback<T>
    {
        /// <summary>
        /// GSS node which uniquely represents parser state within a GSS layer.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="layer"></param>
        /// <param name="stage">
        /// Each GSS layer can be filled during shift which initially fills
        /// layer (stage #0) and during subsequent incoming tokens which can
        /// cause reductions via lookahaead conditions.  For LALR(1) lookahead
        /// is only one character hence there are only 2 stages #0 - for shifts
        /// (and for shift-reduces), #1 - for reduces triggered by the next
        /// incoming term.
        /// </param>
        public GssNode(State state, int layer, GssStage stage, int lookahead = -1)
        {
            this.State = state;
            this.Layer = layer;
            this.Stage = stage;
            this.Lookahead = lookahead;
        }

        public GssBackLink<T> BackLink { get; private set; }

        /// <summary>
        /// Depth of path until a first back link ambiguity
        /// </summary>
        public int      DeterministicDepth { get; internal set; } = 1;

        public State    State     { get; }

        public int      Layer     { get; }

        public GssStage Stage     { get; }

        public int      Lookahead { get; }

        public int LinkCount 
        { 
            get 
            {
                int count = 0;
                var link = BackLink;
                while (link != null)
                {
                    ++count;
                    link = link.Alternative;
                }

                return count;
            } 
        }

        public GssBackLink<T> PushLinkAlternative(GssNode<T> leftNode, T label)
        {
            var result = new GssBackLink<T>(leftNode, label, BackLink);
            BackLink = result;
            return result;
        }

        State IStackLookback<T>.GetParentState() { return State; }

        State IStackLookback<T>.GetState(int backoffset)
        {
            var node = GetNodeAtDepth(backoffset);
            return node.State;
        }

        T IStackLookback<T>.GetNodeAt(int backoffset)
        {
            var node = GetNodeAtDepth(backoffset);
            return node.BackLink.Label;
        }

        private GssNode<T> GetNodeAtDepth(int depth)
        {
            Debug.Assert(depth > 0, "Depth should be at least 1");
            Debug.Assert(this.DeterministicDepth >= depth, "Non-deterministic lookback.");

            GssNode<T> node = this;

            while (0 != --depth)
            {
                node = node.BackLink.PriorNode;
            }

            return node;
        }

        public GssBackLink<T> ResolveBackLink(GssNode<T> priorNode)
        {
            return BackLink
                .Alternatives()
                .ResolveFirst(l => l.PriorNode == priorNode);
        }

        public override string ToString()
        {
            return string.Format("GssNode(State={0})", State);
        }
    }
}
