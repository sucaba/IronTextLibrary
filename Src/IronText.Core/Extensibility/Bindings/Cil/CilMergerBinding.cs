using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework.Reflection;

namespace IronText.Extensibility.Bindings.Cil
{
    class CilMergerBinding : IMergerBinding
    {
        public MergeActionBuilder Builder { get; set; }
    }
}