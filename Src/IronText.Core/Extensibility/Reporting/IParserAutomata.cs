using System.Collections.ObjectModel;
using IronText.Framework;
using System.Collections.Generic;

namespace IronText.Extensibility
{
    public interface IParserAutomata
    {
        ReadOnlyCollection<IParserState> States { get; }

        ReadOnlyCollection<ParserConflictInfo> Conflicts { get; }
    }
}