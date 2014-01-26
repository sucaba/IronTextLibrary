using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Reflection;

namespace IronText.Analysis
{
    interface IEbnfConverter
    {
        T Convert<T>(T source) where T : SymbolBase;

        Production Convert(Production source);
    }
}
