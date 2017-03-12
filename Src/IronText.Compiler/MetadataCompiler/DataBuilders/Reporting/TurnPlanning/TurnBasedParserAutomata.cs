using System.Collections.ObjectModel;
using IronText.Automata.TurnPlanning;
using IronText.Reporting;
using IronText.Common;
using System.Linq;
using IronText.Reflection;

namespace IronText.MetadataCompiler.DataBuilders.Reporting.TurnPlanning
{
    class TurnBasedParserAutomata : IParserAutomata
    {
        public ReadOnlyCollection<IParserState> States { get; }

        private readonly ShrodingerTokenDfaProvider       dfaProvider;
        private readonly Indexer<ShrodingerTokenDfaState> stateIndexer;
        private readonly Grammar                          grammar;

        public TurnBasedParserAutomata(
            ShrodingerTokenDfaProvider       dfaProvider,
            ReturnLookaheadProvider          returnLaProvider,
            Indexer<ShrodingerTokenDfaState> stateIndexer,
            Grammar                          grammar,
            TurnBasedNameProvider            turnNameProvider)
        {
            this.dfaProvider  = dfaProvider;
            this.stateIndexer = stateIndexer;
            this.grammar      = grammar;

            GraphImplMap<ShrodingerTokenDfaState, TurnBasedParserAutomataState> mapping = null;

            mapping = new GraphImplMap<ShrodingerTokenDfaState, TurnBasedParserAutomataState>(
                (state, reportState) =>
                    reportState.Init(
                        grammar,
                        turnNameProvider,
                        stateIndexer[state],
                        state,
                        dfaProvider.Details[state],
                        returnLaProvider,
                        mapping.Of));

            mapping.EnsureMapped(dfaProvider.States);

            this.States = mapping
                .Values
                .Cast<IParserState>()
                .ToList()
                .AsReadOnly();
        }
    }
}