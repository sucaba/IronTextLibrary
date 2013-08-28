using System;
using IronText.Framework;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace IronText.Extensibility
{
    public interface ILanguageData
    {
        LanguageName LanguageName { get; }

        BnfGrammar Grammar { get; }

        int TokenCount { get; }

        ReadOnlyCollection<DotState> ParserStates { get; }

        ParserAction GetParserAction(int state, int token);

        ReadOnlyCollection<ParserConflictInfo> GetParserConflicts();

        IEnumerable<ParserAction> GetConflictActions(int conflictIndex, int count);

        string GetDestinationDirectory();
    }
}
