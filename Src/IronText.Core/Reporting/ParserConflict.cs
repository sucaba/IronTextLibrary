namespace IronText.Reporting
{
    public class ParserConflict
    {
        internal ParserConflict(IParserState state, IParserTransition transition)
        {
            this.State      = state;
            this.Transition = transition;
        }

        public IParserState      State      { get; }

        public IParserTransition Transition { get; }
    }
}
