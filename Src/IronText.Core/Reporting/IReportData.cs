using System;
using IronText.Framework;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using IronText.Reflection;
using IronText.Runtime;

namespace IronText.Reporting
{
    /// <summary>
    /// Contract for the language's reporting data
    /// </summary>
    public interface IReportData
    {
        string          DestinationDirectory { get; }

        LanguageName    Name { get; }

        Grammar         Grammar { get; }

        IParserAutomata ParserAutomata { get; }
    }
}
