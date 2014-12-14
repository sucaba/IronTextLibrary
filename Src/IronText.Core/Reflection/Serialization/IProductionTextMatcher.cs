using System.Collections.Generic;

namespace IronText.Reflection
{
    internal interface IProductionTextMatcher
    {
        bool Match(Production production, ProductionSketch sketch);
    }
}
