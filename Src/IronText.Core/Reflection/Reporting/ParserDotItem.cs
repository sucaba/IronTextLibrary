using System.Collections.Generic;

namespace IronText.Reflection.Reporting
{
    public class ParserDotItem : IParserDotItem
    {
        public ParserDotItem(Production production, int position, IEnumerable<int> la)
        {
            this.Production = production;
            this.Position   = position;
            this.LA         = la;
        }

        public Production       Production { get; private set; }

        public int              Position   { get; private set; }

        public IEnumerable<int> LA         { get; private set; }
    }
}
