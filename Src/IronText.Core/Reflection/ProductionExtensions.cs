using IronText.Runtime;
using System.Linq;

namespace IronText.Reflection
{
    public static class ProductionExtensions
    {
        public static RuntimeProduction ToRuntime(this Production production)
        {
            return new RuntimeProduction(
                production.Index,
                production.Outcome.Index,
                production.InputTokens);
        }
    }
}
