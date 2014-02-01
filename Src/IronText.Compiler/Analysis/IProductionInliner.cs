using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Reflection;

namespace IronText.Analysis
{
    internal interface IProductionInliner
    {
        Grammar Inline();
    }
}
