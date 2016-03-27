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
        private HLoc _parsingHLocation;
        private HLoc _scanningHLocation;
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

            return new ActionNode(0, null, HLoc.Unknown, inh);
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
                _scanningHLocation = envelope.HLocation;

                value = termFactory(context, data.Action, data.Text);
            }

            return new ActionNode(
                data.Token,
                value,
                envelope.HLocation);
        }

        public ActionNode CreateBranch(RuntimeProduction prod, ArraySlice<ActionNode> prefix, IStackLookback<ActionNode> stackLookback)
        {
            if (prefix.Count == 0)
            {
                return GetDefault(prod.Outcome, stackLookback);
            }

            HLoc hLocation;

            var array = prefix.Array;
            switch (prefix.Count)
            {
                case 0: 
                    hLocation = HLoc.Unknown;
                    break;
                case 1: 
                    hLocation = prefix.Array[prefix.Offset].HLocation;
                    break;
                default: 
                    hLocation = prefix.Array[prefix.Offset].HLocation
                             + prefix.Array[prefix.Offset + prefix.Count - 1].HLocation; 
                    break;
            }

            // Location for IParsing service
            this._parsingHLocation = hLocation;

            if (prod.InputLength > prefix.Count)
            {
                throw new NotSupportedException();
            }


            var result = new ActionNode(prod.Outcome, null, hLocation);

            RuntimeFormula[] formulas = grammar.GetReduceFormulas(prod.Index);
            foreach (var formula in formulas)
            {
                formula.Execute(stackLookback, result);
            }

            var pargs = new ProductionActionArgs(prod.Index, prefix.Array, prefix.Offset, prefix.Count, context, stackLookback, result);
            result.Value = productionAction(pargs);
            return result;
        }

        public ActionNode Merge(ActionNode alt1, ActionNode alt2, IStackLookback<ActionNode> stackLookback)
        {
            var result = new ActionNode(
                    alt1.Token,
                    this.merge(alt1.Token, alt1.Value, alt2.Value, context, stackLookback),
                    alt1.HLocation);
            return result;
        }

        HLoc IParsing.HLocation { get { return this._parsingHLocation; } }

        HLoc IScanning.HLocation { get { return this._scanningHLocation; } }

        public IProducer<ActionNode> GetRecoveryProducer()
        {
            return NullProducer<ActionNode>.Instance;
        }

        public void Shifted(IStackLookback<ActionNode> lookback)
        {
            int shiftedState = lookback.GetParentState();
            RuntimeFormula[] formulas = grammar.GetShiftedFormulas(shiftedState);
            foreach (var formula in formulas)
            {
                formula.Execute(lookback);
            }

            shiftAction(lookback);
        }
    }
}
