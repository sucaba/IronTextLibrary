using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework;
using IronText.Runtime;

namespace IronText.Reporting
{
    public interface IParserTransition
    {
        int Token { get; }

        IEnumerable<ParserAction> Actions { get; }
    }
}
