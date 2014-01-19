using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework.Reflection;

namespace IronText.Extensibility
{
    public class CilScanConditionDef
    {
        public CilScanConditionDef(Type type)
        {
            this.ConditionType = type;
        }

        public Type ConditionType { get; private set; }
    }
}
