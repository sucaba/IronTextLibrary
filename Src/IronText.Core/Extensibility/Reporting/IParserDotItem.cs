using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework;
using IronText.Framework.Reflection;

namespace IronText.Extensibility
{
    public interface IParserDotItem
    {
        Production Rule { get; }

        int Position { get; }

        IEnumerable<int> Lookaheads { get; }
    }
}
