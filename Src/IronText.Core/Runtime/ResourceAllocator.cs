
namespace IronText.Runtime
{
    sealed class ResourceAllocator : IResourceAllocator
    {
        private readonly RuntimeGrammar grammar;

        public ResourceAllocator(RuntimeGrammar grammar)
        {
            this.grammar = grammar;
        }

        public object[] AllocateRuleValuesBuffer()
        {
            return new object[grammar.MaxRuleSize];
        }
    }
}
