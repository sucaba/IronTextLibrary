
namespace IronText.Framework
{
    struct ModifiedReduction
    { 
        public readonly BnfRule Rule;
        public readonly short Size;

        public ModifiedReduction(BnfRule rule, short size)
        {
            this.Rule = rule;
            this.Size = size;
        }
    }
}
