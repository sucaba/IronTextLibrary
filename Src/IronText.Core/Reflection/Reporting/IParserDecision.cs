namespace IronText.Reflection.Reporting
{
    public interface IParserDecision
    {
        string ActionText { get; }

        IParserState   NextState { get; }
    }
}
