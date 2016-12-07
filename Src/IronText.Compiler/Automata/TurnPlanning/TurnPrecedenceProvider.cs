using IronText.Reflection;

namespace IronText.Automata.TurnPlanning
{
    class TurnPrecedenceProvider
    {
        private readonly Grammar grammar;

        public TurnPrecedenceProvider(Grammar grammar)
        {
            this.grammar = grammar;
        }

        public Precedence GetPrecedence(Turn turn)
        {
            return InternalGetWeight((dynamic)turn);
        }

        private Precedence InternalGetPecedence(InnerReductionTurn turn)
        {
            return grammar.Productions[turn.ProductionId].EffectivePrecedence;
        }

        private Precedence InternalGetPecedence(ReturnTurn turn)
        {
            return grammar.Productions[turn.ProducedToken].EffectivePrecedence;
        }

        private Precedence InternalGetPecedence(InputConsumptionTurn turn)
        {
            return grammar.Symbols[turn.Token].Precedence;
        }

        private Precedence InternalGetWeight(AcceptanceTurn turn)
        {
            return null;
        }
    }
}
