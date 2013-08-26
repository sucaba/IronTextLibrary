using IronText.Algorithm;

namespace IronText.Automata.Lalr1
{
    interface ILrParserTable
    {
        ITable<int> GetParserActionTable();

        int[] GetConflictActionTable();
    }
}
