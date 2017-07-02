using IronText.Reflection;
using System;

namespace IronText.Framework
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple=true)]
    public class ProduceTopDownAttribute : ProduceAttributeBase
    {
        public ProduceTopDownAttribute(int precedence, params string[] keywordMask) 
            : base(precedence: precedence, keywordMask: keywordMask)
        {
        }

        public ProduceTopDownAttribute(Associativity assoc, params string[] keywordMask) 
            : base(assoc: assoc, keywordMask:keywordMask)
        {
        }

        public ProduceTopDownAttribute(int precedence, Associativity assoc, params string[] keywordMask) 
            : base(precedence, assoc, keywordMask)
        {
        }

        public ProduceTopDownAttribute(params string[] keywordMask) 
            : base(keywordMask: keywordMask)
        {
        }
    }
}
