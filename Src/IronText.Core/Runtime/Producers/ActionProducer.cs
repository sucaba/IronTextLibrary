using System.Diagnostics;
using IronText.Algorithm;
using IronText.Framework;
using IronText.Logging;

namespace IronText.Runtime
{
    sealed class ActionProducer 
        : ActionEpsilonProducer
        , IProducer<Msg>
        , IParsing
    {
        private readonly BnfGrammar grammar;
        private readonly GrammarActionDelegate grammarAction;
        private readonly MergeDelegate merge;
        private readonly object context;
        private Loc _resultLocation;
        private HLoc _resultHLocation;
        private readonly object[] ruleArgBuffer;

        public ActionProducer(
            BnfGrammar grammar,
            object context,
            GrammarActionDelegate grammarAction,
            MergeDelegate merge)
            : base(grammar, context, grammarAction)
        {
            this.grammar        = grammar;
            this.context        = context;
            this.grammarAction  = grammarAction;
            this.merge          = merge;
            this.ruleArgBuffer  = new object[grammar.MaxRuleSize];
        }

        public ReductionOrder ReductionOrder { get { return ReductionOrder.ByRuleDependency; } }

        public Msg Result { get; set; }

        public Msg CreateLeaf(Msg leaf)
        {
            return leaf;
        }

        public Msg CreateBranch(BnfRule rule, ArraySlice<Msg> prefix, IStackLookback<Msg> stackLookback)
        {
            if (prefix.Count == 0)
            {
                return GetEpsilonNonTerm(rule.Left, stackLookback);
            }

            Loc location;
            HLoc hLocation;

            var array = prefix.Array;
            switch (prefix.Count)
            {
                case 0: 
                    location = Loc.Unknown;
                    hLocation = HLoc.Unknown;
                    break;
                case 1: 
                    location = prefix.Array[prefix.Offset].Location;
                    hLocation = prefix.Array[prefix.Offset].HLocation;
                    break;
                default: 
                    location = prefix.Array[prefix.Offset].Location
                             + prefix.Array[prefix.Offset + prefix.Count - 1].Location; 
                    hLocation = prefix.Array[prefix.Offset].HLocation
                             + prefix.Array[prefix.Offset + prefix.Count - 1].HLocation; 
                    break;
            }

            // Location for IParsing service
            this._resultLocation = location;
            this._resultHLocation = hLocation;

            if (rule.Parts.Length > prefix.Count)
            {
                FillEpsilonSuffix(rule.Id, prefix.Count, prefix.Array, prefix.Offset + prefix.Count, stackLookback);
            }

            object value = grammarAction(rule, prefix.Array, prefix.Offset, context, stackLookback);

            return new Msg(rule.Left, value, location, hLocation);
        }

        public Msg Merge(Msg alt1, Msg alt2, IStackLookback<Msg> stackLookback)
        {
#if DIAGNOSTICS
            Debug.WriteLine("Merging token {0} values:", (object)grammar.TokenName(alt1.Id));
            Debug.WriteLine(" '{0}'", alt1.Value);
            Debug.WriteLine("and");
            Debug.WriteLine(" '{0}'", alt2.Value);
#endif
            var result = new Msg(
                    alt1.Id,
                    this.merge(alt1.Id, alt1.Value, alt2.Value, context, stackLookback),
                    alt1.Location);

#if DIAGNOSTICS
            Debug.WriteLine("Merged value = '{0}'", (object)result.Value);
#endif
            return result;
        }

        Loc IParsing.Location
        {
            get { return this._resultLocation; }
        }

        HLoc IParsing.HLocation
        {
            get { return this._resultHLocation; }
        }

        public IProducer<Msg> GetErrorRecoveryProducer()
        {
            return NullProducer<Msg>.Instance;
        }
    }
}
