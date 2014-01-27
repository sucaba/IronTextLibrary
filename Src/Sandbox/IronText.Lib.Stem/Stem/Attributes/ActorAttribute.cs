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
    public class ActorAttribute : RuleMethodAttribute
    {
        public ActorAttribute(params string[] keywordMask) { this.KeywordMask = keywordMask; }

        /// <summary>
        /// Name in s-expressions or null if name is default
        /// </summary>
        public string[] KeywordMask { get; set; }

        protected override CilSymbolRef[] DoGetRuleMask(MethodInfo methodInfo)
        {
            var resultList = new List<CilSymbolRef>();

            resultList.Add(CilSymbolRef.Create(StemScanner.LParen));

            resultList.AddRange(KeywordMask.Select(item => item == null ? null : CilSymbolRef.Create(item)));

            int nonMaskParameterCount = methodInfo.GetParameters().Length - KeywordMask.Count(item => item == null);
            if (nonMaskParameterCount < 0)
            {
                throw new InvalidOperationException("Insufficient rule-method arguments.");
            }

            resultList.AddRange(Enumerable.Repeat(default(CilSymbolRef), nonMaskParameterCount));

            resultList.Add(CilSymbolRef.Create(StemScanner.RParen));

            return resultList.ToArray(); ;
        }
    }
}
