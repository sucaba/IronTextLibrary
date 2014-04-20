using System.Diagnostics;
using System.Linq;
using IronText.Logging;

namespace IronText.Runtime
{
    class ActionEpsilonProducer
    {
        private readonly RuntimeGrammar       grammar;
        private readonly ProductionActionDelegate productionAction;
        private readonly object                   context;

        public ActionEpsilonProducer(RuntimeGrammar grammar, object context, ProductionActionDelegate productionAction)
        {
            this.grammar          = grammar;
            this.context          = context;
            this.productionAction = productionAction;
        }

        public StackNode GetDefault(int nonTerm, IStackLookback<StackNode> stackLookback)
        {
            var result = InternalGetNullable(nonTerm, stackLookback);
            return result;
        }

        private StackNode InternalGetNullable(int nonTerm, IStackLookback<StackNode> stackLookback)
        {
            Debug.Assert(grammar.IsNullable(nonTerm));

            var production = 
                      (from r in grammar.GetProductions(nonTerm)
                       where r.PatternTokens.All(grammar.IsNullable)
                       orderby r.PatternTokens.Length ascending
                       select r)
                       .First();

            var args = new StackNode[production.PatternTokens.Length];
            for (int i = 0; i != args.Length; ++i)
            {
                args[i] = InternalGetNullable(production.PatternTokens[i], stackLookback);
            }

            var value = productionAction(production.Index, args, 0, context, stackLookback);
            return new StackNode(nonTerm, value, Loc.Unknown, HLoc.Unknown);
        }

        public void FillEpsilonSuffix(int prodId, int prefixSize, StackNode[] buffer, int destIndex, IStackLookback<StackNode> stackLookback)
        {
            var production = grammar.Productions[prodId];
            int i   = prefixSize;
            int end = production.PatternTokens.Length;
            while (i != end)
            {
                buffer[destIndex++] = GetDefault(production.PatternTokens[i++], stackLookback);
            }
        }
    }
}
