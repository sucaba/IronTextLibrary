namespace IronText.Automata.TurnPlanning
{
    class UnknownTurn : Turn
    {
        public static UnknownTurn Instance = new UnknownTurn();

        private UnknownTurn() { }

        public override bool Equals(Turn other) => ReferenceEquals(this, other);
    }
}
