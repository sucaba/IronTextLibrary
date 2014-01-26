using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework;
using IronText.Reflection;

namespace IronText.Extensibility
{
    public interface IParserDotItem
    {
        Production Production { get; }

        int Position { get; }

        IEnumerable<int> LA { get; }
    }
}
