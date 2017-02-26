using IronText.Reflection;
using System;
using System.Collections.Generic;

namespace IronText.Reporting
{
    class ParserDotItem : IParserDotItem
    {
        readonly Production production;

        public ParserDotItem(Production production, int position, IEnumerable<string> la)
        {
            this.production = production;
            this.Position   = position;
            this.LA         = la;
        }

        public int              Position    { get; }

        public IEnumerable<string> LA       { get; }

        public int              ProductionIndex => production.Index;

        public string           Outcome => production.Outcome.Name;

        public int              InputLength => production.InputLength;

        public string[]         Input =>
            Array.ConvertAll(production.Input, s => s.Name);
    }
}
