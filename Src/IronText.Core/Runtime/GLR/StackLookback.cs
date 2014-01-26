using System.Diagnostics;

namespace IronText.Runtime
{
    public static class StackLookback
    {
        public static Msg GssLookback(object glrLookback, int count)
        {
            GssNode<Msg> node = (GssNode<Msg>)glrLookback;
            Debug.Assert(count > 0,  "Lookback should be at least 1 token back");
            Debug.Assert(node.DeterministicDepth >= count,  "Non-deterministic lookback.");

            GssLink<Msg> lastLink = null;
            GssNode<Msg> lastNode = null;

            do
            {
                lastNode = node;
                lastLink = node.FirstLink;
                node = lastLink.LeftNode;
            }
            while (--count != 0);

            // Make new Msg to replace token Id with state Id
            return new Msg(lastNode.State, lastLink.Label.Value, lastLink.Label.Location);
        }
    }
}
