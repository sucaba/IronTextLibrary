using IronText.Algorithm;

namespace IronText.Framework
{
    sealed class SppfProducer 
        : SppfEpsilonProducer
        , IProducer<SppfNode>
    {
        public SppfProducer(BnfGrammar grammar)
            : base(grammar)
        {
        }

        public ReductionOrder ReductionOrder { get { return ReductionOrder.Unordered; } }

        public SppfNode Result { get; set; }

        public SppfNode CreateLeaf(Msg envelope, MsgData data)
        { 
            return new SppfNode(data.TokenId, data.Value, envelope.Location, envelope.HLocation);
        }

        public SppfNode CreateBranch(
                            BnfRule      rule,
                            ArraySlice<SppfNode> parts,
                            IStackLookback<SppfNode> stackLookback)
        {
            // Produce more dense tree
            if (parts.Count == 0)
            {
                return GetEpsilonNonTerm(rule.Left, stackLookback);
            }

            SppfNode[] children = new SppfNode[rule.Parts.Length];
            parts.CopyTo(children, 0);

            Loc location = Loc.Unknown;
            int partCount = parts.Count;
            for (int i = 0; i != partCount; ++i)
            {
                location += children[i].Location;
            }

            if (rule.Parts.Length > parts.Count)
            {
                FillEpsilonSuffix(rule.Id, parts.Count, children, parts.Count, stackLookback);
            }

            return new SppfNode(rule.Id, location, children);
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
