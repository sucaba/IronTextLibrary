namespace IronText.Automata.TurnPlanning
{
    class EnterTurn : Turn
    {
        public EnterTurn(int token)
        {
            this.ProducedToken = token;
        }

        public int ProducedToken { get; }

        public override bool Equals(Turn other)
        {
            var similar = other as EnterTurn;
            return (object)similar != null
                && similar.ProducedToken == ProducedToken;
        }

        public override int GetHashCode() => ~ProducedToken;
    }
}
