using IronText.Automata.Lalr1;
using IronText.Reflection.Reporting;

namespace IronText.MetadataCompiler
{
    class ParserTableProvider
    {
        public ParserTableProvider(
            ReportCollection        reports,
            ParserRuntimeDesignator runtimeDesignator,
            CanonicalLrDfaTable     lrTable,
            ConflictMessageBuilder  conflictMessageBuilder)
        {
            this.LrParserTable = lrTable;

            if (!runtimeDesignator.ComplyWithConfiguration)
            {
                reports.Add(conflictMessageBuilder);
            }
        }

        public ILrParserTable LrParserTable { get; }
    }
}
