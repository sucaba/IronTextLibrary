using IronText.Collections;

namespace IronText.Runtime
{
    static class DeterministicDepthUpdater
    {
        public static void OnLinkAdded<T>(ImmutableArray<GssNode<T>> front, GssNode<T> toNode)
        {
            var link      = toNode.BackLink;
            var priorNode = link.PriorNode;

            if (link.IsDeterminisic)
            {
                toNode.DeterministicDepth = priorNode.DeterministicDepth + 1;
            }
            else
            {
                toNode.DeterministicDepth = 0;
                Update(front);
            }
        }

        public static void Update<T>(ImmutableArray<GssNode<T>> front)
        {
            int changes;
            do
            {
                changes = 0;
                // Note: Just added link can affect deterministic 
                //       depth of the GSS front nodes only.
                foreach (var topNode in front)
                {
                    int newDepth = Compute(topNode);
                    if (newDepth != topNode.DeterministicDepth)
                    {
                        topNode.DeterministicDepth = newDepth;
                        ++changes;
                    }
                }
            }
            while (changes != 0);
        }

        private static int Compute<T>(GssNode<T> node)
        {
            if (node.BackLink == null)
            {
                return 1;
            }

            if (node.BackLink.IsDeterminisic)
            {
                return node.BackLink.PriorNode.DeterministicDepth + 1;
            }

            return 0;
        }
    }
}
