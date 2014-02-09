using System.Collections.Generic;

namespace IronText.Reflection.Reporting
{
    public interface IParserDotItem
    {
        Production Production { get; }

        int Position { get; }

        IEnumerable<int> LA { get; }
    }
}
