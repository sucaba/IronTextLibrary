using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Runtime
{
    internal interface ILanguageInternalRuntime
    {
        RuntimeGrammar RuntimeGrammar { get; }
    }
}
