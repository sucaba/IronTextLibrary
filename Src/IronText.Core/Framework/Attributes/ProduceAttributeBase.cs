using IronText.Reflection;
using IronText.Reflection.Managed;
using System;
using System.Linq;
using System.Reflection;

namespace IronText.Framework
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple=true, Inherited = true)]
    public abstract class ProduceAttributeBase : RuleMethodAttribute
    {
        public ProduceAttributeBase(
            int           precedence  = 0,
            Associativity assoc       = Associativity.None,
            string[]      keywordMask = null) 
        {
            this.Precedence    = precedence;
            this.Associativity = assoc;
            this.KeywordMask   = keywordMask ?? new string[0];
        }

        public string[] KeywordMask { get; set; }

        protected override CilSymbolRef[] DoGetRuleMask(MethodInfo methodInfo)
        {
            int placeholderCount = KeywordMask.Count(item => item == null);
            int nonPlaceholderParameterCount = methodInfo.GetParameters().Length - placeholderCount;
            if (nonPlaceholderParameterCount < 0)
            {
                throw new InvalidOperationException("Insufficient rule-method arguments in " + this);
            }

            return KeywordMask
                .Select(item => item == null ? null : CilSymbolRef.Create(item))
                .ToArray();
        }
    }
}
