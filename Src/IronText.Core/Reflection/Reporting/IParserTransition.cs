using System.Collections.Generic;

namespace IronText.Reflection.Reporting
{
    public interface IParserTransition
    {
        string Symbol { get; }

        IEnumerable<IParserDecision> AlternateDecisions { get; }
    }
}
