using IronText.Algorithm;
using IronText.Framework;
using IronText.Logging;
using IronText.Reflection;
using System;
using System.Collections.Generic;

namespace IronText.Runtime
{
    sealed class ActionProducer 
        : ActionEpsilonProducer
        , IProducer<ActionNode>
        , IParsing
        , IScanning
    {
        private readonly RuntimeGrammar grammar;
        private readonly ProductionActionDelegate productionAction;
        private readonly TermFactoryDelegate termFactory;
        private readonly MergeDelegate merge;
        private readonly object context;
        private Loc  _parsingLocation;
        private HLoc _parsingHLocation;
        private Loc  _scanningLocation;
        private HLoc _scanningHLocation;
        private readonly object[] ruleArgBuffer;
        private Dictionary<string, object> globals;

        public ActionProducer(
            RuntimeGrammar           grammar,
            object                   context,
            ProductionActionDelegate productionAction,
            TermFactoryDelegate      termFactory,
            MergeDelegate            merge,
            Dictionary<string,object> globals)
            : base(grammar, context, productionAction)
        {
            this.grammar        = grammar;
            this.context        = context;
            this.productionAction  = productionAction;
            this.termFactory    = termFactory;
            this.merge          = merge;
            this.ruleArgBuffer  = new object[grammar.MaxRuleSize];
            this.globals        = globals ?? new Dictionary<string,object>();

            if (context != null)
            {
                ServicesInitializer.SetServiceProperties(
                    context.GetType(),
                    context,
                    typeof(IParsing),
                    this);

                ServicesInitializer.SetServiceProperties(
                    context.GetType(),
                    context,
                    typeof(IScanning),
                    this);
            }
        }

        public ReductionOrder ReductionOrder { get { return ReductionOrder.ByRuleDependency; } }

        public ActionNode Result { get; set; }

        public ActionNode CreateStart()
        {
            PropertyValueNode inh = null;
            foreach (var pair in globals)
            {
                inh = new PropertyValueNode(pair.Key, pair.Value).SetNext(inh);
            }

            return new ActionNode(0, null, Loc.Unknown, HLoc.Unknown, inh);
        }

        public ActionNode CreateLeaf(Msg envelope, MsgData data)
        {
            object value;
            if (data.Action < 0)
            {
                value = data.ExternalValue;
            }
            else
            {
                _scanningLocation  = envelope.Location;
                _scanningHLocation = envelope.HLocation;

                value = termFactory(context, data.Action, data.Text);
            }

            return new ActionNode(
                data.Token,
                value,
                envelope.Location,
                envelope.HLocation);
        }

        public ActionNode CreateBranch(Production prod, ArraySlice<ActionNode> prefix, IStackLookback<ActionNode> stackLookback)
        {
            if (prefix.Count == 0)
            {
                return GetDefault(prod.OutcomeToken, stackLookback);
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
            this._parsingLocation = location;
            this._parsingHLocation = hLocation;

            if (prod.InputLength > prefix.Count)
            {
                throw new NotSupportedException();
            }

            var pargs = new ProductionActionArgs(prod.Index, prefix.Array, prefix.Offset, context, stackLookback);
            object value = productionAction(pargs);

            return new ActionNode(prod.OutcomeToken, value, location, hLocation);
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

        Loc IParsing.Location { get { return this._parsingLocation; } }

        HLoc IParsing.HLocation { get { return this._parsingHLocation; } }

        Loc IScanning.Location { get { return this._scanningLocation; } }

        HLoc IScanning.HLocation { get { return this._scanningHLocation; } }

        public IProducer<ActionNode> GetErrorRecoveryProducer()
        {
            return NullProducer<ActionNode>.Instance;
        }
    }
}
