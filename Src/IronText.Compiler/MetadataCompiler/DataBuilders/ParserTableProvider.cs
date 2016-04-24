using IronText.Automata.Lalr1;
using IronText.Logging;
using IronText.Reflection;

namespace IronText.MetadataCompiler
{
    class ParserTableProvider
    {
        public ParserTableProvider(
            ILrDfa         dfa, 
            RuntimeOptions flags,
            Grammar        grammar,
            ILogging       logging)
        {
            var lrTable = new ConfigurableLrTable(dfa, flags);
            if (!lrTable.ComplyWithConfiguration)
            {
                grammar.Reports.Add(new ConflictMessageBuilder(logging));
            }

            this.LrParserTable = lrTable;
        }

        public ILrParserTable LrParserTable { get; private set; }
    }
}
