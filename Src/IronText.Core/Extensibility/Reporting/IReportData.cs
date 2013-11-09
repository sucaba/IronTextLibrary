using System;
using IronText.Framework;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using IronText.Framework.Reflection;

namespace IronText.Extensibility
{
    /// <summary>
    /// Contract for the language's reporting data
    /// </summary>
    public interface IReportData
    {
        string DestinationDirectory { get; }

        LanguageName Name { get; }

        EbnfGrammar Grammar { get; }

        int TokenCount { get; }

        ReadOnlyCollection<ScanMode> ScanModes { get; }

        IScannerAutomata GetScanModeDfa(Type scanModeType);

        IParserAutomata ParserAutomata { get; }
    }
}
