using System.Collections.Generic;
using IronText.Reflection;

namespace IronText.Reporting
{
    public interface IParserDotItem
    {
        Production Production { get; }

        int Position { get; }

        IEnumerable<int> LA { get; }
    }
}
