namespace IronText.Automata.TurnPlanning
{
    class ReturnTurn : Turn
    {
        public ReturnTurn(int token)
        {
            this.ProducedToken = token;
        }

        public int ProducedToken { get; }

        public override bool Equals(Turn other)
        {
            var similar = other as ReturnTurn;
            return (object)similar != null
                && similar.ProducedToken == ProducedToken;
        }

        public override int GetHashCode() => ProducedToken;
    }
}
