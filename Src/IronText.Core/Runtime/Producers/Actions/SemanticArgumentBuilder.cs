using IronText.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Misc;

namespace IronText.Runtime.Producers.Actions
{
    internal class SemanticArgumentBuilder
    {
        private readonly ActionNode[] nodes;
        private readonly int firstIndex;
        private readonly object[] output;
        private readonly IStackLookback<ActionNode> lookback;
        private int currentIndex;
        private int outputIndex;

        public SemanticArgumentBuilder(
            ActionNode[] nodes,
            int          firstIndex,
            object[]     output,
            IStackLookback<ActionNode> lookback)
        {
            this.nodes        = nodes;
            this.firstIndex   = firstIndex;
            this.currentIndex = firstIndex;
            this.output       = output;
            this.outputIndex  = 0;
            this.lookback     = lookback;
        }

        public void FillSemanticParameters(IProductionComponent root)
        {
            Production              prod;
            Symbol                  symbol;
            InjectedActionParameter placeholder;

            switch (root.Match(out prod, out symbol, out placeholder))
            {
                case 0: 
                    foreach (var child in prod.ChildComponents)
                    {
                        FillSemanticParameters(child);
                    }
                    break;
                case 1:
                    output[outputIndex++] = nodes[currentIndex++].Value;
                    break;
                case 2:
                    foreach (var node in REnumerateNodes())
                    {
                        var props = node.FollowingStateProperties;
                        object value;
                        if (props.TryGetValue(placeholder.Name, out value))
                        {
                            output[outputIndex++] = value;
                            break;
                        }
                    }

                    break;
            }
        }

        private IEnumerable<ActionNode> REnumerateNodes()
        {
            if (currentIndex < nodes.Length)
            {
                int topmostIndex = currentIndex;
                for (;topmostIndex >= firstIndex; --topmostIndex)
                {
                    yield return nodes[topmostIndex];
                }
            }
            else
            {
                for(int i = 0; ;)
                {
                    yield return lookback.GetNodeAt(++i);
                }
            }
        }
    }
}
