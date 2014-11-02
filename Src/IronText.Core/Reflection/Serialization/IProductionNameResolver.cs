using System.Collections.Generic;

namespace IronText.Reflection
{
    internal interface IProductionNameResolver
    {
        Production Resolve(ProductionSketch sketch, bool createMissing = false);

        Production Resolve(string text, bool createMissing = false);

        Production Resolve(string outcome, IEnumerable<string> components, bool createMissing = false);
    }
}
