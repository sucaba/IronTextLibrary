using IronText.Algorithm;
using IronText.Reflection.Reporting;
using IronText.Runtime;

namespace IronText.Automata.Lalr1
{
    interface ILrParserTable
    {
        ITable<ParserDecision> ParserActionTable { get; }

        ParserConflictInfo[] Conflicts         { get; }
    }
}
