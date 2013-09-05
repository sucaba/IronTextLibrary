using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Extensibility;
using System.Collections.ObjectModel;

namespace IronText.MetadataCompiler
{
    class ParserAutomata : IParserAutomata
    {
        private readonly LanguageData data;
        private ReadOnlyCollection<IParserState> states;

        public ParserAutomata(LanguageData data)
        {
            this.data = data;
        }

        public ReadOnlyCollection<IParserState> States
        {
            get 
            {
                if (states == null)
                {
                    int count = data.ParserStates.Length;
                    var array = new ParserState[count];
                    for (int i = 0; i != count; ++i)
                    {
                        array[i] = new ParserState(data.ParserStates[i], data);
                    }

                    states = new ReadOnlyCollection<IParserState>(array);
                }

                return states;
            }
        }

        public ReadOnlyCollection<ParserConflictInfo> Conflicts
        {
            get { return new ReadOnlyCollection<ParserConflictInfo>(data.Lalr1Conflicts); }
        }
    }
}
