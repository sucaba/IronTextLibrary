using IronText.Algorithm;
using IronText.Framework;
using IronText.Logging;
using IronText.Runtime.Semantics;
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
        private readonly ShiftActionDelegate shiftAction;
        private readonly object context;
        private Loc _parsingLocation;
        private Loc _scanningLocation;
        private readonly object[] ruleArgBuffer;
        private Dictionary<int, object> inhIndexToValue;

        public ActionProducer(
            RuntimeGrammar           grammar,
            object                   context,
            ProductionActionDelegate productionAction,
            TermFactoryDelegate      termFactory,
            ShiftActionDelegate      shiftAction,
            MergeDelegate            merge,
            Dictionary<int,object>   inhIndexToValue)
            : base(grammar, context, productionAction)
        {
            this.grammar        = grammar;
            this.context        = context;
            this.productionAction  = productionAction;
            this.termFactory    = termFactory;
            this.merge          = merge;
            this.ruleArgBuffer  = new object[grammar.MaxProductionLength];
            this.inhIndexToValue        = inhIndexToValue ?? new Dictionary<int,object>();

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

            if (shiftAction == null)
            {
                this.shiftAction = lookback => { };
            }
            else
            {
                this.shiftAction = shiftAction;
            }
        }

        public ReductionOrder ReductionOrder { get { return ReductionOrder.ByRuleDependency; } }

        public ActionNode Result { get; set; }

        public ActionNode CreateStart()
        {
            PropertyValueNode inh = null;
            foreach (var pair in inhIndexToValue)
            {
                inh = new PropertyValueNode(pair.Key, pair.Value).SetNext(inh);
            }

            return new ActionNode(0, null, Loc.Unknown, inh);
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
                _scanningLocation = envelope.Location;

                value = termFactory(context, data.Action, data.Text);
            }

            return new ActionNode(
                data.Token,
                value,
                envelope.Location);
        }

        public ActionNode CreateBranch(RuntimeProduction prod, ArraySlice<ActionNode> prefix, IStackLookback<ActionNode> stackLookback)
        {
            if (prefix.Count == 0)
            {
                return GetDefault(prod.Outcome, stackLookback);
            }

            Loc location;

            var array = prefix.Array;
            switch (prefix.Count)
            {
                case 0:
                    location = Loc.Unknown;
                    break;
                case 1:
                    location = prefix.Array[prefix.Offset].Location;
                    break;
                default:
                    location = prefix.Array[prefix.Offset].Location
                             + prefix.Array[prefix.Offset + prefix.Count - 1].Location;
                    break;
            }

            // Location for IParsing service
            this._parsingLocation = location;

            if (prod.InputLength > prefix.Count)
            {
                throw new NotSupportedException();
            }

            var result = new ActionNode(prod.Outcome, null, location);

            new RuntimeReduceSemantics(grammar)
                .Execute(prod.Index, stackLookback, result);

            var pargs = new ProductionActionArgs(
                            prod.Index,
                            prefix.Array,
                            prefix.Offset,
                            prefix.Count,
                            context,
                            stackLookback,
                            result);
            result.Value = productionAction(pargs);
            return result;
        }

        public ActionNode Merge(ActionNode alt1, ActionNode alt2, IStackLookback<ActionNode> stackLookback)
        {
            var result = new ActionNode(
                    alt1.Token,
                    this.merge(alt1.Token, alt1.Value, alt2.Value, context, stackLookback),
                    alt1.Location);
            return result;
        }

        public void Shifted(IStackLookback<ActionNode> lookback)
        {
            new RuntimeShiftSemantics(grammar)
                .Execute(lookback);

            shiftAction(lookback);
        }

        Loc IParsing.Location { get { return this._parsingLocation; } }

        Loc IScanning.Location { get { return this._scanningLocation; } }

        public IProducer<ActionNode> GetRecoveryProducer()
        {
            return NullProducer<ActionNode>.Instance;
        }
    }
}
