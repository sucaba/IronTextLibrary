using System.Collections.Generic;

namespace IronText.Reflection
{
    internal interface IProductionParser
    {
        Production       ParseProduction(string text);

        Production       ParseProduction(string outcome, IEnumerable<string> pattern);

        ProductionSketch BuildSketch(string text);
    }
}
