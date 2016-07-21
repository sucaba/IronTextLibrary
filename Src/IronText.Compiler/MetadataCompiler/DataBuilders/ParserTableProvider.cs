using IronText.Automata.Lalr1;
using IronText.Reflection.Reporting;

namespace IronText.MetadataCompiler
{
    class ParserTableProvider
    {
        public ParserTableProvider(
            ReportCollection       reports,
            ConfigurableLrTable    lrTable,
            ConflictMessageBuilder conflictMessageBuilder)
        {
            this.LrParserTable = lrTable;

            if (!lrTable.ComplyWithConfiguration)
            {
                reports.Add(conflictMessageBuilder);
            }
        }

        public ILrParserTable LrParserTable { get; }
    }
}
