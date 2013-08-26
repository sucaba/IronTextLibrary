using System.Collections.Generic;

namespace IronText.Extensibility
{
    public class ParserConflictInfo
    {
        public int          State;
        public int          Token;
        public readonly List<ParserAction> Actions = new List<ParserAction>();
    }
}
