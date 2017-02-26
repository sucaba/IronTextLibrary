using System;
using System.Collections.ObjectModel;
using IronText.Automata.TurnPlanning;
using IronText.Reporting;
using System.Linq;
using IronText.Collections;

/*
namespace IronText.MetadataCompiler.DataBuilders.Reporting.TurnPlanning
{
    class TurnParserAutomata : IParserAutomata
    {
        private ShrodingerTokenDfaProvider dfaProvider;

        public TurnParserAutomata(ShrodingerTokenDfaProvider dfaProvider)
        {
            this.dfaProvider = dfaProvider;
        }

        public ReadOnlyCollection<ParserConflictInfo> Conflicts
        {
            get
            {
                return dfaProvider.States
                    .SelectMany(x => x
                        .Transitions
                        .Where(t => t.Value.IsAmbiguous)
                        .SelectMany(t => t
                            .Value.AllAlternatives()
                            .Select(a => new
                            {
                                state = ToText(x),
                                token = t.Key,
                                turn = ToText(a.Turn),
                                nextState = ToText(a.NextState)
                            })))
                           .ToList();
            }
        }

        private object ToText(ShrodingerTokenDfaState x)
        {
            throw new NotImplementedException();
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
*/