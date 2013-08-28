using IronText.Framework;

namespace IronText.Framework
{
    struct ModifiedReduction
    { 
        public BnfRule Rule;
        public short Size;

        public ModifiedReduction(BnfRule rule, short size)
        {
            this.Rule = rule;
            this.Size = size;
        }
    }
}
