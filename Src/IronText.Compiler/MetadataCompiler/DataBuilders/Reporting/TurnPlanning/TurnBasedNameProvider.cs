using System;
using IronText.Automata.TurnPlanning;
using IronText.Reflection;

namespace IronText.MetadataCompiler.DataBuilders.Reporting.TurnPlanning
{
    class TurnBasedNameProvider
    {
        private readonly Grammar grammar;

        public TurnBasedNameProvider(Grammar grammar)
        {
            this.grammar = grammar;
        }

        public string NameOfSymbol(int outcome)
        {
            return grammar.Symbols.NameOf(outcome);
        }

        public string NameOfTurn(Turn turn)
        {
            return TurnText((dynamic)turn);
        }

        private string TurnText(Turn turn)
        {
            throw new NotImplementedException($"{turn.GetType().FullName}");
        }

        private string TurnText(InputConsumptionTurn turn)
        {
            return $"shift-{grammar.Symbols.NameOf(turn.TokenToConsume.Value)}";
        }

        private string TurnText(EnterTurn turn)
        {
            return $"enter-{grammar.Symbols.NameOf(turn.ProducedToken)}";
        }

        private string TurnText(ReturnTurn turn)
        {
            return $"return-{grammar.Symbols.NameOf(turn.ProducedToken)}";
        }

        private string TurnText(TopDownReductionTurn turn)
        {
            return $"ll-reduce-{turn.ProductionId}";
        }

        private string TurnText(BottomUpReductionTurn turn)
        {
            return $"lr-reduce-{turn.ProductionId}";
        }

        private string TurnText(AcceptanceTurn turn)
        {
            return "accept";
        }
    }
}
