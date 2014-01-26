using IronText.Algorithm;
using System.Collections.ObjectModel;
using IronText.Extensibility;
using IronText.Reporting;

namespace IronText.Automata.Lalr1
{
    interface ILrParserTable
    {
        bool RequiresGlr { get; }

        ITable<int> GetParserActionTable();

        int[] GetConflictActionTable();

        ParserConflictInfo[] Conflicts { get; }
    }
}
