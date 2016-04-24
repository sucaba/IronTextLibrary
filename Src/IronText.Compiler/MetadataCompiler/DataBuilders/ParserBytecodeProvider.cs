using IronText.Automata.Lalr1;
using IronText.Runtime;

namespace IronText.MetadataCompiler
{
    class ParserBytecodeProvider
    {
        public ParserBytecodeProvider(ILrParserTable parserTable)
        {
        }

        public ParserAction[] Instructions { get; }
    }
}
