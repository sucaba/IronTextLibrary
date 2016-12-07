namespace IronText.Automata.TurnPlanning
{
    class InnerReductionTurn : Turn
    {
        public InnerReductionTurn(int productionId)
        {
            this.ProductionId = productionId;
        }

        public int ProductionId { get; }

        public override bool Equals(Turn other)
        {
            var similar = other as InnerReductionTurn;
            return similar != null
                && similar.ProductionId == ProductionId;
        }

        public override int GetHashCode() => ~ProductionId;
    }
}
