using IronText.Algorithm;
using IronText.Compiler.Analysis;
using IronText.Runtime;

namespace IronText.Automata.Lalr1
{
    class CanonicalLrDfaTable : ILrParserTable
    {
        private readonly GrammarAnalysis            grammar;
        private readonly ParserConflictResolver     conflictResolver;
        private readonly LrTableBuilder builder;

        public CanonicalLrDfaTable(
            ILrDfa          dfa,
            GrammarAnalysis grammar,
            ParserConflictResolver conflictResolver,
            LrMainTableFiller fillMainTable,
            LrAlternateTermsTableFiller alternateTerms)
        {
            this.grammar = grammar;
            this.conflictResolver = conflictResolver;
            this.builder = new LrTableBuilder(
                                conflictResolver,
                                dfa.States.Length,
                                grammar.TotalSymbolCount);

            fillMainTable.Apply(builder);

            HasUnresolvedTerminalAmbiguities = alternateTerms.Apply(builder);
        }

        public bool HasUnresolvedTerminalAmbiguities { get; }

        public ITable<ParserDecision> DecisionTable => builder.GetResult();
    }
}
