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
        string DestinationDirectory { get; }

        LanguageName Name { get; }

        BnfGrammar Grammar { get; }

        int TokenCount { get; }

        int ParserStateCount { get; }

        ReadOnlyCollection<ScanMode> ScanModes { get; }

        ReadOnlyCollection<DotState> ParserStates { get; }

        ReadOnlyCollection<ParserConflictInfo> ParserConflicts { get; }

        ITdfaData GetScanModeDfa(Type scanModeType);

        ParserAction GetParserAction(int state, int token);

        IEnumerable<ParserAction> GetConflictActions(int conflictIndex, int count);

        IEnumerable<ParserAction> GetAllParserActions(int state, int token);
    }
}
