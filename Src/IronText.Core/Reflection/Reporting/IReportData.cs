using IronText.Runtime;

namespace IronText.Reflection.Reporting
{
    /// <summary>
    /// Contract for a language report data source.
    /// </summary>
    public interface IReportData
    {
        string              DestinationDirectory { get; }

        ILanguageSource     Source           { get; }

        Grammar             Grammar          { get; }

        IParserAutomata     ParserAutomata   { get; }

        ISemanticBinding[]  SemanticBindings { get; }

        IScannerAutomata    GetScannerAutomata();
    }
}
