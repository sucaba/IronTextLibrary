using System.Collections.Generic;

namespace IronText.Reporting
{
    public interface IParserTransition
    {
        string Symbol { get; }

        IEnumerable<IParserDecision> AlternateDecisions { get; }
    }
}
