using System.Collections.Generic;

namespace IronText.Reflection.Reporting
{
    public interface IParserDotItem
    {
        int         ProductionIndex { get; }

        string      Outcome         { get; }

        string[]    Input           { get; }

        int         InputLength     { get; }

        int         Position        { get; }

        IEnumerable<string> LA      { get; }
    }
}
