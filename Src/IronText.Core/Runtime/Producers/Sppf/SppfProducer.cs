using IronText.Algorithm;
using IronText.Logging;
using IronText.Reflection;

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

        public SppfNode CreateLeaf(Msg envelope, MsgData data)
        { 
            return new SppfNode(data.Action, data.Text, envelope.Location, envelope.HLocation);
        }

        public SppfNode CreateBranch(
                            Production           prod,
                            ArraySlice<SppfNode> parts,
                            IStackLookback<SppfNode> stackLookback)
        {
            // Produce more dense tree
            if (parts.Count == 0)
            {
                return GetDefault(prod.OutcomeToken, stackLookback);
            }

            SppfNode[] children = new SppfNode[prod.Input.Length];
            parts.CopyTo(children, 0);

            if (prod.InputTokens.Length > parts.Count)
            {
                FillEpsilonSuffix(prod.Index, parts.Count, children, parts.Count, stackLookback);
            }

            Loc location;
            int partsCount = parts.Count;
            switch (partsCount)
            {
                case 0:
                    location = Loc.Unknown;
                    break;
                case 1:
                    location = children[0].Location;
                    break;
                default:
                    location = new Loc(children[0].Location.Position, children[partsCount - 1].Location.End);
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

        public IProducer<SppfNode> GetErrorRecoveryProducer() { return this; }
    }
}
