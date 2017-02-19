namespace IronText.Automata.TurnPlanning
{
    class InputConsumptionTurn : Turn
    {
        public InputConsumptionTurn(int token)
        {
            this.Token = token;
        }

        public int Token { get; }

        public override int? TokenToConsume => Token;

        public override bool Equals(Turn other)
        {
            var similar = other as InputConsumptionTurn;
            return (object)similar != null
                && similar.Token == Token;
        }

        public override int GetHashCode() => Token;
    }
}
