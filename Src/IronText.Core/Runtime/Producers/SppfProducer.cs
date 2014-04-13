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
            return new SppfNode(data.Token, data.Text, envelope.Location, envelope.HLocation);
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

            SppfNode[] children = new SppfNode[prod.Pattern.Length];
            parts.CopyTo(children, 0);

            Loc location = Loc.Unknown;
            int partCount = parts.Count;
            for (int i = 0; i != partCount; ++i)
            {
                location += children[i].Location;
            }

            if (prod.PatternTokens.Length > parts.Count)
            {
                FillEpsilonSuffix(prod.Index, parts.Count, children, parts.Count, stackLookback);
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
