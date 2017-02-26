using System.Collections.ObjectModel;

namespace IronText.Reporting
{
    public interface IParserAutomata
    {
        ReadOnlyCollection<IParserState> States { get; }

        ReadOnlyCollection<ParserConflictInfo> Conflicts { get; }
    }
}