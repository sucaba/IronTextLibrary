using IronText.Reflection;
using IronText.Reflection.Managed;

namespace IronText.Reporting
{
    /// <summary>
    /// Contract for a language report data source.
    /// </summary>
    public interface IReportData
    {
        string              DestinationDirectory { get; }

        IGrammarSource      Source         { get; }

        Grammar             Grammar        { get; }

        IParserAutomata     ParserAutomata { get; }

        IScannerAutomata    GetScannerAutomata(Condition condition);
    }
}
