using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework.Reflection;

namespace IronText.Analysis
{
    internal interface IProductionInliner
    {
        EbnfGrammar Inline();
    }
}
