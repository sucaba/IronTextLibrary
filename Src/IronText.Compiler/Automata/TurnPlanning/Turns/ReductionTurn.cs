namespace IronText.Automata.TurnPlanning
{
    class ReductionTurn : Turn
    {
        public ReductionTurn(int productionId)
        {
            this.ProductionId = productionId;
        }

        public int ProductionId { get; }

        public override bool Equals(Turn other)
        {
            var similar = other as ReductionTurn;
            return similar != null
                && similar.ProductionId == ProductionId;
        }

        public override int GetHashCode() => ~ProductionId;
    }
}
