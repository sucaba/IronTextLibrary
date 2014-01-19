using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IronText.Extensibility;
using IronText.Framework;

namespace IronText.Lib.Stem
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ClsAttribute : RuleMethodAttribute
    {
        public ClsAttribute() { }

        protected override CilSymbolRef[] DoGetRuleMask(MethodInfo methodInfo, ITokenPool tokenPool)
        {
            var resultList = new List<CilSymbolRef>();

            resultList.AddRange(
                Enumerable.Repeat(default(CilSymbolRef),
                methodInfo.GetParameters().Length));

            resultList.Add(tokenPool.GetLiteral(StemScanner.RParen));

            return resultList.ToArray(); ;
        }
    }
}
