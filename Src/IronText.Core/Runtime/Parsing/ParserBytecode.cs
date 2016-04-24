using System;

namespace IronText.Runtime.Parsing
{
    [Serializable]
    public class ParserBytecode
    {
        public ParserBytecode(ParserAction[] instructions)
        {
            this.Instructions = instructions;
        }

        public ParserAction[] Instructions { get; }
    }
}