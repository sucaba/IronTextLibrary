using IronText.Framework;

namespace IronText.Runtime
{
    sealed class ResourceAllocator : IResourceAllocator
    {
        private readonly BnfGrammar grammar;

        public ResourceAllocator(BnfGrammar grammar)
        {
            this.grammar = grammar;
        }

        public object[] AllocateRuleValuesBuffer()
        {
            return new object[grammar.MaxRuleSize];
        }
    }
}
