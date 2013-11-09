using IronText.Framework.Reflection;

namespace IronText.Framework
{
    sealed class ResourceAllocator : IResourceAllocator
    {
        private readonly EbnfGrammar grammar;

        public ResourceAllocator(EbnfGrammar grammar)
        {
            this.grammar = grammar;
        }

        public object[] AllocateRuleValuesBuffer()
        {
            return new object[grammar.MaxRuleSize];
        }
    }
}
