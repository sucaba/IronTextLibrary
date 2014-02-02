using System;
using IronText.Framework;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using IronText.Reflection;
using IronText.Runtime;

namespace IronText.Reporting
{
    /// <summary>
    /// Contract for a language report data source.
    /// </summary>
    public interface IReportData
    {
        string          DestinationDirectory { get; }

        LanguageName    Name           { get; }

        Grammar         Grammar        { get; }

        IParserAutomata ParserAutomata { get; }

        IScannerAutomata GetScannerAutomata(Condition condition);
    }
}
