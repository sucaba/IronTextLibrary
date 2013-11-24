using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Framework
{
    internal interface ILanguageRuntime
    {
        RuntimeEbnfGrammar RuntimeGrammar { get; }
    }
}
