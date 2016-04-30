using IronText.Algorithm;
using IronText.Automata.Lalr1;
using IronText.Runtime;
using System.Collections.Generic;

namespace IronText.MetadataCompiler
{
    class ParserBytecodeProvider
    {
        public ParserBytecodeProvider(ILrParserTable parserTable)
        {
            var instructions = new List<ParserAction>();

            var table = parserTable.GetParserActionTable();
            int rowCount    = table.RowCount;
            int columnCount = table.ColumnCount;

            var startTable = new MutableTable<int>(rowCount, columnCount);

            for (int r = 0; r != rowCount; ++r)
                for (int c = 0; c != columnCount; ++c)
                {
                    int start = instructions.Count;
                    startTable.Set(r, c, start);
                    instructions.Add(table.Get(r, c));
                    //instructions.Add(ParserAction.ExitAction);
                }

            this.Instructions = instructions.ToArray();
            this.StartTable   = startTable;
        }

        public ParserAction[] Instructions { get; }

        public ITable<int>    StartTable   { get; }
    }
}
