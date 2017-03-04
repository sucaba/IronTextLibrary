using IronText.Reporting;

namespace IronText.MetadataCompiler.DataBuilders.Reporting.TurnPlanning
{
    class TurnBasedParserDecision : IParserDecision
    {
        public TurnBasedParserDecision(string actionText, IParserState nextState)
        {
            ActionText = actionText;
            NextState  = nextState;
        }

        public string       ActionText { get; }

        public IParserState NextState  { get; }
    }
}
