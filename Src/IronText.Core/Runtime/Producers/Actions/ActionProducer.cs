using IronText.Algorithm;
using IronText.Framework;
using IronText.Logging;
using IronText.Reflection;

namespace IronText.Runtime
{
    sealed class ActionProducer 
        : ActionEpsilonProducer
        , IProducer<ActionNode>
        , IParsing
    {
        private readonly RuntimeGrammar grammar;
        private readonly ProductionActionDelegate grammarAction;
        private readonly TermFactoryDelegate termFactory;
        private readonly MergeDelegate merge;
        private readonly object context;
        private Loc _resultLocation;
        private HLoc _resultHLocation;
        private readonly object[] ruleArgBuffer;

        public ActionProducer(
            RuntimeGrammar           grammar,
            object                   context,
            ProductionActionDelegate grammarAction,
            TermFactoryDelegate      termFactory,
            MergeDelegate            merge)
            : base(grammar, context, grammarAction)
        {
            this.grammar        = grammar;
            this.context        = context;
            this.grammarAction  = grammarAction;
            this.termFactory    = termFactory;
            this.merge          = merge;
            this.ruleArgBuffer  = new object[grammar.MaxRuleSize];
        }

        public ReductionOrder ReductionOrder { get { return ReductionOrder.ByRuleDependency; } }

        public ActionNode Result { get; set; }

        public ActionNode CreateLeaf(Msg envelope, MsgData data)
        {
            return new ActionNode(
                data.Token,
                data.Action < 0 ? data.ExternalValue : termFactory(context, data.Action, data.Text),
                envelope.Location,
                envelope.HLocation);
        }

        public ActionNode CreateBranch(Production rule, ArraySlice<ActionNode> prefix, IStackLookback<ActionNode> stackLookback)
        {
            if (prefix.Count == 0)
            {
                return GetDefault(rule.OutcomeToken, stackLookback);
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

            if (rule.PatternTokens.Length > prefix.Count)
            {
                FillEpsilonSuffix(rule.Index, prefix.Count, prefix.Array, prefix.Offset + prefix.Count, stackLookback);
            }

            object value = grammarAction(rule.Index, prefix.Array, prefix.Offset, context, stackLookback);

            return new ActionNode(rule.OutcomeToken, value, location, hLocation);
        }

        public ActionNode Merge(ActionNode alt1, ActionNode alt2, IStackLookback<ActionNode> stackLookback)
        {
            var result = new ActionNode(
                    alt1.Token,
                    this.merge(alt1.Token, alt1.Value, alt2.Value, context, stackLookback),
                    alt1.Location,
                    alt1.HLocation);
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

        public IProducer<ActionNode> GetErrorRecoveryProducer()
        {
            return NullProducer<ActionNode>.Instance;
        }
    }
}
