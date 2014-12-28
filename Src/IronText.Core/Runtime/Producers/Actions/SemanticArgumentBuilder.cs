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
        private readonly ProductionActionArgs pargs;
        private readonly object[] output;
        private int outputIndex;
        private int currentIndex;

        public SemanticArgumentBuilder(
            ProductionActionArgs pargs,
            object[]     output,
            int          outputIndex)
        {
            this.pargs        = pargs;
            this.currentIndex = 0;
            this.output       = output;
            this.outputIndex  = outputIndex;
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
                    output[outputIndex++] = pargs.GetSyntaxArg(currentIndex++).Value;
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
            if (currentIndex < pargs.SyntaxArgCount)
            {
                int topmostIndex = currentIndex;
                for (;topmostIndex >= 0; --topmostIndex)
                {
                    yield return pargs.GetSyntaxArg(topmostIndex);
                }
            }
            else
            {
                for(int i = 0; ;)
                {
                    yield return pargs.Lookback.GetNodeAt(++i);
                }
            }
        }
    }
}
