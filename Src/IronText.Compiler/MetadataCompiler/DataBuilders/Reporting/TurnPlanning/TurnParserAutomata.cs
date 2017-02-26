using System;
using System.Collections.ObjectModel;
using IronText.Automata.TurnPlanning;
using IronText.Reporting;
using System.Linq;
using IronText.Collections;
using IronText.Common;

namespace IronText.MetadataCompiler.DataBuilders.Reporting.TurnPlanning
{
    class TurnParserAutomata : IParserAutomata
    {
        private readonly ShrodingerTokenDfaProvider       dfaProvider;
        private readonly Indexer<ShrodingerTokenDfaState> stateIndexer;

        public TurnParserAutomata(
            ShrodingerTokenDfaProvider       dfaProvider,
            Indexer<ShrodingerTokenDfaState> stateIndexer)
        {
            this.dfaProvider  = dfaProvider;
            this.stateIndexer = stateIndexer;
        }

        public ReadOnlyCollection<IParserState> States
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}