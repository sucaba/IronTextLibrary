using System.Collections.Generic;

namespace IronText.Runtime
{
    using System.Diagnostics;
    using State = System.Int32;

    sealed class GssNode<T> : IStackLookback<T>
    {
        public int DeterministicDepth = 1;
        public readonly State State;
        public readonly int Layer;
        public readonly byte Stage;
        public readonly int Lookahead;
        private GssLink<T> firstLink;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="layer"></param>
        /// <param name="stage">
        /// Each GSS layer is can be filled during shift 
        /// which initially fills layer (stage #0) and 
        /// during subsequent incoming tokens which can
        /// cause reductions via lookahaead conditions.
        /// For LALR(1) lookahead is only one character
        /// hence there are only 2 stages #0 - for shifts (and for shift-reduces),
        /// #1 - for reduces triggered by the next incoming
        /// term.
        /// </param>
        public GssNode(State state, int layer, byte stage, int lookahead = -1)
        {
            this.State = state;
            this.Layer = layer;
            this.Stage = stage;
            this.Lookahead = lookahead;
        }

        public int LinkCount 
        { 
            get 
            {
                int count = 0;
                var link = FirstLink;
                while (link != null)
                {
                    ++count;
                    link = link.NextLink;
                }

                return count;
            } 
        }

        public IEnumerable<GssLink<T>> Links
        {
            get
            {
                var link = firstLink;
                while (link != null)
                {
                    yield return link;
                    link = link.NextLink;
                }
            }
        }

        public GssLink<T> AddLink(GssNode<T> leftNode, T label)
        {
            var result = new GssLink<T>(leftNode, label, firstLink);
            firstLink = result;
            return result;
        }

        public GssLink<T> FirstLink
        {
            get
            {
                //Debug.Assert(DeterministicDepth > 0);
                return firstLink;
            }
        }

        public int ComputeDeterministicDepth()
        {
            if (firstLink == null)
            {
                return 1;
            }

            if (firstLink.NextLink == null)
            {
                return firstLink.LeftNode.DeterministicDepth + 1;
            }

            return 0;
        }

        State IStackLookback<T>.GetParentState() { return State; }

        State IStackLookback<T>.GetTopState() { return State; }

        T IStackLookback<T>.GetNodeAt(int count)
        {
            GssNode<T> node = this;

            Debug.Assert(count > 0,  "Lookback should be at least 1 token back");
            Debug.Assert(node.DeterministicDepth >= count,  "Non-deterministic lookback.");

            GssLink<T> lastLink = null;
            GssNode<T> lastNode = null;

            do
            {
                lastNode = node;
                lastLink = node.FirstLink;
                node = lastLink.LeftNode;
            }
            while (--count != 0);

            return lastLink.Label;
        }

        public override string ToString()
        {
            return string.Format("GssNode(State={0})", State);
        }
    }
}
