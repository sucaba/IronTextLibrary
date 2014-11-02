using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Reflection
{
    internal interface IProductionTextMatcher
    {
        bool MatchProduction(Production production, ProductionSketch sketch);

        bool MatchProduction(Production production, string text);

        bool MatchProduction(Production p, string outcome, IEnumerable<string> pattern);
    }
}
