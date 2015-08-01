using System.Diagnostics;
using System.Linq;
using IronText.Logging;

namespace IronText.Runtime
{
    class ActionEpsilonProducer
    {
        private readonly RuntimeGrammar           grammar;
        private readonly ProductionActionDelegate productionAction;
        private readonly object                   context;

        public ActionEpsilonProducer(RuntimeGrammar grammar, object context, ProductionActionDelegate productionAction)
        {
            this.grammar          = grammar;
            this.context          = context;
            this.productionAction = productionAction;
        }

        public ActionNode GetDefault(int nonTerm, IStackLookback<ActionNode> stackLookback)
        {
            var result = InternalGetNullable(nonTerm, stackLookback);
            return result;
        }

        private ActionNode InternalGetNullable(int nonTerm, IStackLookback<ActionNode> stackLookback)
        {
            Debug.Assert(grammar.IsNullable(nonTerm));

            var production = grammar.GetNullableProductions(nonTerm).First();

            int len = production.InputLength;
            var args = new ActionNode[len];
            for (int i = 0; i != len; ++i)
            {
                args[i] = InternalGetNullable(production.Input[i].Index, stackLookback);
            }

            var resultNode = new ActionNode(nonTerm, null, Loc.Unknown, HLoc.Unknown);
            var pargs = new ProductionActionArgs(production.Index, args, 0, production.InputLength, context, stackLookback, resultNode);
            resultNode.Value = productionAction(pargs);
            return resultNode;
        }
    }
}
