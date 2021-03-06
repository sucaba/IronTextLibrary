﻿using System.Collections.ObjectModel;
using IronText.Automata.Lalr1;
using IronText.MetadataCompiler;

namespace IronText.Reflection.Reporting
{
    class ParserAutomata : IParserAutomata
    {
        private readonly LanguageData data;
        private ReadOnlyCollection<IParserState> states;
        private readonly ParserConflictInfo[] parserConflicts;
        private readonly DotState[] parserStates;

        public ParserAutomata(ReportData reportData)
        {
            this.data            = reportData.data;
            this.parserConflicts = reportData.parserConflicts;
            this.parserStates    = reportData.parserStates;
        }

        public ReadOnlyCollection<IParserState> States
        {
            get 
            {
                if (states == null)
                {
                    int count = parserStates.Length;
                    var array = new ParserState[count];
                    for (int i = 0; i != count; ++i)
                    {
                        array[i] = new ParserState(parserStates[i], data);
                    }

                    states = new ReadOnlyCollection<IParserState>(array);
                }

                return states;
            }
        }

        public ReadOnlyCollection<ParserConflictInfo> Conflicts
        {
            get { return new ReadOnlyCollection<ParserConflictInfo>(parserConflicts); }
        }
    }
}
