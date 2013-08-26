using System;
using System.Linq;
using System.Reflection;
using IronText.Extensibility;

namespace IronText.Framework
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple=true)]
    public class ParseResultAttribute : RuleMethodAttribute
    {
        public ParseResultAttribute(params string[] keywordMask) 
        {
            this.KeywordMask = keywordMask ?? new string[0];
        }

        public string[] KeywordMask { get; set; }

        protected override MethodInfo Method
        {
            get
            {
                var property = (PropertyInfo)Member;
                var getter = property.GetSetMethod();
                if (getter == null)
                {
                    var msg = string.Format("Property '{0}' should have setter method.", property);
                    throw new InvalidOperationException(msg);
                }

                return getter;
            }
        }

        protected override TokenRef[] DoGetRuleMask(MethodInfo methodInfo, ITokenPool tokenPool)
        {
            int placeholderCount = KeywordMask.Count(item => item == null);
            int nonPlaceholderParameterCount = methodInfo.GetParameters().Length - placeholderCount;
            if (nonPlaceholderParameterCount < 0)
            {
                throw new InvalidOperationException("Insufficient rule-method arguments in " + this);
            }

            return KeywordMask
                .Select(item => item == null ? null : tokenPool.GetLiteral(item))
                .ToArray();
        }
    }
}
