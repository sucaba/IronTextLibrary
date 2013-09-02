using System.Collections.Generic;
using System.Linq;

namespace IronText.Framework
{
    using System.Diagnostics;
    using State = System.Int32;

    sealed class GssNode<T> : IStackLookback<T>
    {
        public int DeterministicDepth = 1;
        public readonly State State;
        public readonly int Layer;
        public readonly byte Stage;
        private GssLink<T> prevLink;

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
        public GssNode(State state, int layer, byte stage)
        {
            State = state;
            Layer = layer;
            Stage = stage;
        }

        public int LinkCount { get { return Links.Count(); } }

        public IEnumerable<GssLink<T>> Links
        {
            get
            {
                var link = prevLink;
                while (link != null)
                {
                    yield return link;
                    link = link.NextSibling;
                }
            }
        }

        public GssLink<T> AddLink(GssNode<T> leftNode, T label)
        {
            var result = new GssLink<T>(leftNode, label, prevLink);
            prevLink = result;
            return result;
        }

        public GssLink<T> PrevLink
        {
            get
            {
                //Debug.Assert(DeterministicDepth > 0);
                return prevLink;
            }
        }

        public int ComputeDeterministicDepth()
        {
            if (prevLink == null)
            {
                return 1;
            }

            if (prevLink.NextSibling == null)
            {
                return prevLink.LeftNode.DeterministicDepth + 1;
            }

            return 0;
        }

        State IStackLookback<T>.GetParentState() { return State; }

        T IStackLookback<T>.GetValueAt(State count)
        {
            GssNode<T> node = this;

            Debug.Assert(count > 0,  "Lookback should be at least 1 token back");
            Debug.Assert(node.DeterministicDepth >= count,  "Non-deterministic lookback.");

            GssLink<T> lastLink = null;
            GssNode<T> lastNode = null;

            do
            {
                lastNode = node;
                lastLink = node.PrevLink;
                node = lastLink.LeftNode;
            }
            while (--count != 0);

            return lastLink.Label;
        }
    }
}
