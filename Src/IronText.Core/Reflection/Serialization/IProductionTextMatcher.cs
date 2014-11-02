using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Reflection
{
    internal interface IProductionTextMatcher
    {
        bool Match(Production production, ProductionSketch sketch);

        bool Match(Production production, string text);

        bool Match(Production p, string outcome, IEnumerable<string> pattern);
    }
}
