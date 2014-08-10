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

            var args = new ActionNode[production.InputTokens.Length];
            for (int i = 0; i != args.Length; ++i)
            {
                args[i] = InternalGetNullable(production.InputTokens[i], stackLookback);
            }

            var value = productionAction(production.Index, args, 0, context, stackLookback);
            return new ActionNode(nonTerm, value, Loc.Unknown, HLoc.Unknown);
        }
    }
}
