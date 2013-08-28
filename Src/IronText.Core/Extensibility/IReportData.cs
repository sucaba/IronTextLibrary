using System;
using IronText.Framework;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace IronText.Extensibility
{
    /// <summary>
    /// Contract for the language's reporting data
    /// </summary>
    public interface IReportData
    {
        LanguageName Name { get; }

        BnfGrammar Grammar { get; }

        int TokenCount { get; }

        int ParserStateCount { get; }

        ReadOnlyCollection<ScanMode> ScanModes { get; }

        ITdfaData GetScanModeDfa(Type scanModeType);

        ReadOnlyCollection<DotState> ParserStates { get; }

        string GetDestinationDirectory();

        ParserAction GetParserAction(int state, int token);

        IEnumerable<ParserAction> GetAllParserActions(int state, int token);

        ReadOnlyCollection<ParserConflictInfo> GetParserConflicts();

        IEnumerable<ParserAction> GetConflictActions(int conflictIndex, int count);
    }
}
