using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IronText.Extensibility;
using IronText.Framework;

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

        protected override TokenRef[] DoGetRuleMask(MethodInfo methodInfo, ITokenPool tokenPool)
        {
            var resultList = new List<TokenRef>();

            resultList.Add(tokenPool.GetLiteral(StemScanner.LParen));

            resultList.AddRange(KeywordMask.Select(item => item == null ? null : tokenPool.GetLiteral(item)));

            int nonMaskParameterCount = methodInfo.GetParameters().Length - KeywordMask.Count(item => item == null);
            if (nonMaskParameterCount < 0)
            {
                throw new InvalidOperationException("Insufficient rule-method arguments.");
            }

            resultList.AddRange(Enumerable.Repeat(default(TokenRef), nonMaskParameterCount));

            resultList.Add(tokenPool.GetLiteral(StemScanner.RParen));

            return resultList.ToArray(); ;
        }
    }
}
