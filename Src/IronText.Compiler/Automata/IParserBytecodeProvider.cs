using IronText.Algorithm;
using IronText.Runtime;

namespace IronText.Automata
{
    interface IParserBytecodeProvider
    {
        ParserInstruction[] Instructions { get; }

        ITable<int>         StartTable   { get; }
    }
}