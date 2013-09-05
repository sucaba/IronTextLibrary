using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework;

namespace IronText.Extensibility
{
    public interface IParserDotItem
    {
        BnfRule Rule { get; }

        int Position { get; }

        IEnumerable<int> Lookaheads { get; }
    }
}
