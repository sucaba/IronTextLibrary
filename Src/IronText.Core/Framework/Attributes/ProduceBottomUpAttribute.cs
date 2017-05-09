using IronText.Reflection;
using System;

namespace IronText.Framework
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple=true)]
    public class ProduceBottomUpAttribute : ProduceAttributeBase
    {
        public ProduceBottomUpAttribute(int precedence, params string[] keywordMask) 
            : base(precedence: precedence, keywordMask: keywordMask)
        {
            base.Flags |= ProductionFlags.BottomUpBehavior;
        }

        public ProduceBottomUpAttribute(Associativity assoc, params string[] keywordMask) 
            : base(assoc: assoc, keywordMask:keywordMask)
        {
            base.Flags |= ProductionFlags.BottomUpBehavior;
        }

        public ProduceBottomUpAttribute(int precedence, Associativity assoc, params string[] keywordMask) 
            : base(precedence, assoc, keywordMask)
        {
            base.Flags |= ProductionFlags.BottomUpBehavior;
        }

        public ProduceBottomUpAttribute(params string[] keywordMask) 
            : base(keywordMask: keywordMask)
        {
            base.Flags |= ProductionFlags.BottomUpBehavior;
        }
    }
}
