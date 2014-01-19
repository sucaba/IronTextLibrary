using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IronText.Extensibility;
using IronText.Lib.Stem;

namespace IronText.Framework
{
    [AttributeUsage(AttributeTargets.Method)]
    public class OpnAttribute : RuleMethodAttribute
    {
        public OpnAttribute(params string[] keywordMask) { this.KeywordMask = keywordMask; }

        public string[] KeywordMask { get; set; }

        protected override CilSymbolRef[] DoGetRuleMask(MethodInfo methodInfo, ITokenPool tokenPool)
        {
            var resultList = new List<CilSymbolRef>();

            int nonMaskParameterCount = methodInfo.GetParameters().Length - KeywordMask.Count(item => item == null);
            if (nonMaskParameterCount < 0)
            {
                throw new InvalidOperationException("Insufficient rule-method arguments.");
            }

            resultList.Add(tokenPool.GetLiteral(StemScanner.LParen));

            resultList.AddRange(
                KeywordMask
                .Select(item => item == null ? null : tokenPool.GetLiteral(item)));

            return resultList.ToArray();
        }
    }
}
