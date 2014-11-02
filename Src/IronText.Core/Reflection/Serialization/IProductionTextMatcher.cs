using System.Collections.Generic;

namespace IronText.Reflection
{
    internal interface IProductionTextMatcher
    {
        bool Match(Production production, ProductionSketch sketch);

        bool Match(Production production, string text);

        bool Match(Production production, string outcome, IEnumerable<string> components);
    }
}
