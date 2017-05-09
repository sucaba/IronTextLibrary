using IronText.Reflection;

namespace IronText.Framework
{
    public class ProduceAttribute : ProduceAttributeBase
    {
        public ProduceAttribute(int precedence, params string[] keywordMask) 
            : base(precedence: precedence, keywordMask: keywordMask)
        {
        }

        public ProduceAttribute(Associativity assoc, params string[] keywordMask) 
            : base(assoc: assoc, keywordMask:keywordMask)
        {
        }

        public ProduceAttribute(int precedence, Associativity assoc, params string[] keywordMask) 
            : base(precedence, assoc, keywordMask)
        {
        }

        public ProduceAttribute(params string[] keywordMask) 
            : base(keywordMask: keywordMask)
        {
        }
    }
}
