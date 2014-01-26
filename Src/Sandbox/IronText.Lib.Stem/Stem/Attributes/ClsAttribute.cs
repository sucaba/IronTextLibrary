using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Reflection.Managed;

namespace IronText.Lib.Stem
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ClsAttribute : RuleMethodAttribute
    {
        public ClsAttribute() { }

        protected override CilSymbolRef[] DoGetRuleMask(MethodInfo methodInfo)
        {
            var resultList = new List<CilSymbolRef>();

            resultList.AddRange(
                Enumerable.Repeat(default(CilSymbolRef),
                methodInfo.GetParameters().Length));

            resultList.Add(CilSymbolRef.Literal(StemScanner.RParen));

            return resultList.ToArray(); ;
        }
    }
}
