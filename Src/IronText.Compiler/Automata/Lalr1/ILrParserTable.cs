using IronText.Algorithm;
using IronText.Reflection.Reporting;
using IronText.Runtime;

namespace IronText.Automata.Lalr1
{
    interface ILrParserTable
    {
        ParserRuntime        TargetRuntime     { get; }

        ITable<ParserInstruction> ParserActionTable { get; }

        ParserConflictInfo[] Conflicts         { get; }
    }
}
