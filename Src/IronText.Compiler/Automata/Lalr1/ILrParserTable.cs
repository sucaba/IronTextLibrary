using IronText.Algorithm;
using System.Collections.ObjectModel;
using IronText.Extensibility;

namespace IronText.Automata.Lalr1
{
    interface ILrParserTable
    {
        ITable<int> GetParserActionTable();

        int[] GetConflictActionTable();

        ParserConflictInfo[] Conflicts { get; }
    }
}
