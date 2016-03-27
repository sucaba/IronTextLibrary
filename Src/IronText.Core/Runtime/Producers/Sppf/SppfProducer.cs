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
            return new SppfNode(data.Action, data.Text, envelope.HLocation);
        }

        public SppfNode CreateBranch(
                            RuntimeProduction        prod,
                            ArraySlice<SppfNode>     parts,
                            IStackLookback<SppfNode> stackLookback)
        {
            // Produce more dense tree
            if (parts.Count == 0)
            {
                return GetDefault(prod.Outcome, stackLookback);
            }

            SppfNode[] children = new SppfNode[prod.InputLength];
            parts.CopyTo(children, 0);

            if (prod.InputLength > parts.Count)
            {
                throw new NotSupportedException();
                // FillEpsilonSuffix(prod.Index, parts.Count, children, parts.Count, stackLookback);
            }

            HLoc location;
            int partsCount = parts.Count;
            switch (partsCount)
            {
                case 0:
                    location = HLoc.Unknown;
                    break;
                case 1:
                    location = children[0].Location;
                    break;
                default:
                    location = children[0].Location + children[partsCount - 1].Location;
                    break;
            }

            return new SppfNode(prod.Index, location, children);
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


        public void Shifted(IStackLookback<SppfNode> lookback)
        {
        }
    }
}
