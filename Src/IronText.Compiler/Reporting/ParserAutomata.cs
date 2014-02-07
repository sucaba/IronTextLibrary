using System.Collections.ObjectModel;
using IronText.MetadataCompiler;

namespace IronText.Reporting
{
    class ParserAutomata : IParserAutomata
    {
        private readonly LanguageData data;
        private ReadOnlyCollection<IParserState> states;
        private readonly ParserConflictInfo[] parserConflicts;

        public ParserAutomata(ReportData reportData)
        {
            this.data = reportData.data;
            this.parserConflicts = reportData.parserConflicts;
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
            get { return new ReadOnlyCollection<ParserConflictInfo>(parserConflicts); }
        }
    }
}
