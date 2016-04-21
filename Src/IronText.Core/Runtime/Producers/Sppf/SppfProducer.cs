using IronText.Algorithm;
using IronText.Logging;
using System;

namespace IronText.Runtime
{
    sealed class SppfProducer 
        : SppfEpsilonProducer
        , IProducer<SppfNode>
    {
        public SppfProducer(RuntimeGrammar grammar)
            : base(grammar)
        {
        }

        public ReductionOrder ReductionOrder { get { return ReductionOrder.Unordered; } }

        public SppfNode Result { get; set; }

        public SppfNode CreateStart() { return null; }

        public SppfNode CreateLeaf(Msg envelope, MsgData data)
        { 
            return new SppfNode(data.Action, data.Text, envelope.Location);
        }

        public SppfNode CreateBranch(
            RuntimeProduction production,
            IStackLookback<SppfNode> lookback)
        {
            int len = production.InputLength;

            // Produce more dense tree
            if (len == 0)
            {
                return GetDefault(production.Outcome, lookback);
            }

            var children = new SppfNode[len];
            lookback.CopyTo(children, len);

            Loc location;
            switch (len)
            {
                case 0:
                    location = Loc.Unknown;
                    break;
                case 1:
                    location = children[0].Location;
                    break;
                default:
                    location = children[0].Location + children[len - 1].Location;
                    break;
            }

            return new SppfNode(production.Index, location, children);
        }

        public SppfNode Merge(SppfNode alt1, SppfNode alt2, IStackLookback<SppfNode> stackLookback)
        {
            var alt = alt1;
            do
            {
                if (alt2.EquivalentTo(alt))
                {
                    return alt1;
                }

                alt = alt.NextAlternative;
            }
            while (alt != null);

            return alt1.AddAlternative(alt2);
        }

        public IProducer<SppfNode> GetRecoveryProducer() { return this; }


        public void Shifted(int topState, IStackLookback<SppfNode> lookback)
        {
        }
    }
}
