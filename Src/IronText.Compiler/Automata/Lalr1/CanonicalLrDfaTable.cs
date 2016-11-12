using IronText.Algorithm;
using IronText.Runtime;

namespace IronText.Automata.Lalr1
{
    class CanonicalLrDfaTable : ILrParserTable
    {
        private readonly IBuildtimeGrammar        grammar;
        private readonly ParserConflictResolver conflictResolver;
        private readonly LrTableBuilder         builder;

        public CanonicalLrDfaTable(
            ILrDfa          dfa,
            IBuildtimeGrammar grammar,
            ParserConflictResolver conflictResolver,
            LrMainTableFiller fillMainTable,
            LrAlternateTermsTableFiller fillAlternateTermsTable)
        {
            this.grammar = grammar;
            this.conflictResolver = conflictResolver;
            this.builder = new LrTableBuilder(
                                conflictResolver,
                                dfa.States.Length,
                                fillMainTable.SymbolColumnCount 
                                + fillAlternateTermsTable.SymbolColumnCount);

            fillMainTable.Apply(builder);

            HasUnresolvedTerminalAmbiguities = !fillAlternateTermsTable.Apply(builder);
        }

        public bool HasUnresolvedTerminalAmbiguities { get; }

        public ITable<ParserDecision> DecisionTable => builder.GetResult();
    }
}
