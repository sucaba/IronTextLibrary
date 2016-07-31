using IronText.Algorithm;
using IronText.Runtime;

namespace IronText.Automata.Lalr1
{
    interface ILrParserTable
    {
        ITable<ParserDecision> DecisionTable { get; }
    }
}
