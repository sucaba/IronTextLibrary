using IronText.Reflection;

namespace IronText.Framework
{
    public class ProduceAttribute : ProduceAttributeBase
    {
        public ProduceAttribute(int precedence, params string[] keywordMask) 
            : base(precedence: precedence, keywordMask: keywordMask)
        {
            base.Flags |= ProductionFlags.AutoBehavior;
        }

        public ProduceAttribute(Associativity assoc, params string[] keywordMask) 
            : base(assoc: assoc, keywordMask:keywordMask)
        {
            base.Flags |= ProductionFlags.AutoBehavior;
        }

        public ProduceAttribute(int precedence, Associativity assoc, params string[] keywordMask) 
            : base(precedence, assoc, keywordMask)
        {
            base.Flags |= ProductionFlags.AutoBehavior;
        }

        public ProduceAttribute(params string[] keywordMask) 
            : base(keywordMask: keywordMask)
        {
            base.Flags |= ProductionFlags.AutoBehavior;
        }
    }
}
