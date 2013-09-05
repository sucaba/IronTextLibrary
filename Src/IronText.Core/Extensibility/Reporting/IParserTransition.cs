using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework;

namespace IronText.Extensibility
{
    public interface IParserTransition
    {
        int Token { get; }

        IEnumerable<ParserAction> Actions { get; }
    }
}
