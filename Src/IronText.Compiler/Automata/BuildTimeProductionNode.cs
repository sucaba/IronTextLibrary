using System;
using IronText.Runtime;

namespace IronText.Automata
{
    [Serializable]
    public class BuildtimeProductionNode
    {
        public BuildtimeProductionNode(int outcome)
            : this(outcome, null)
        {
        }

        public BuildtimeProductionNode(
            int outcome,
            BuildtimeProductionNode[] components)
        {
            this.Outcome = outcome;
            this.Components = components;
        }

        public int                     Outcome    { get; }

        public BuildtimeProductionNode[] Components { get; }
    }
}
