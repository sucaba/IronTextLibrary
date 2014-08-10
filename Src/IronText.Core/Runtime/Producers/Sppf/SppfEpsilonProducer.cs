using IronText.Logging;
using System.Diagnostics;
using System.Linq;

namespace IronText.Runtime
{
    class SppfEpsilonProducer
    {
        private readonly RuntimeGrammar grammar;
        private SppfNode[] tokenCache;

        public SppfEpsilonProducer(RuntimeGrammar grammar)
        {
            this.grammar = grammar;
            BuildCache();
        }

        private void BuildCache()
        {
            this.tokenCache = new SppfNode[grammar.LastToken];

            foreach (int token in grammar.EnumerateTokens())
            {
                if (grammar.IsNullable(token))
                {
                    tokenCache[token] = InternalGetNullable(token);
                }
            }
        }

        private SppfNode InternalGetNullable(int nonTerm)
        {
            Debug.Assert(grammar.IsNullable(nonTerm));

            var production = grammar.GetNullableProductions(nonTerm).First();

            var args = new SppfNode[production.InputTokens.Length];
            for (int i = 0; i != args.Length; ++i)
            {
                args[i] = InternalGetNullable(production.InputTokens[i]);
            }

            return new SppfNode(production.Index, Loc.Unknown, args);
        }

        public SppfNode GetDefault(int nonTerm, IStackLookback<SppfNode> stackLookback)
        {
            return tokenCache[nonTerm];
        }
    }
}
