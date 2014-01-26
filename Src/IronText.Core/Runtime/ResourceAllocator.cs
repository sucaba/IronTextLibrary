using IronText.Reflection;

namespace IronText.Runtime
{
    sealed class ResourceAllocator : IResourceAllocator
    {
        private readonly RuntimeEbnfGrammar grammar;

        public ResourceAllocator(RuntimeEbnfGrammar grammar)
        {
            this.grammar = grammar;
        }

        public object[] AllocateRuleValuesBuffer()
        {
            return new object[grammar.MaxRuleSize];
        }
    }
}
