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
                    InternalGetNullable(token);
                }
            }
        }

        private SppfNode InternalGetNullable(int nonTerm)
        {
            Debug.Assert(grammar.IsNullable(nonTerm));

            SppfNode result = tokenCache[nonTerm];

            if (result == null)
            {
                var production = grammar.GetNullableProductions(nonTerm).First();

                int[] input = production.InputTokens.ToArray();
                int   len   = input.Length;
                var args = new SppfNode[len];
                for (int i = 0; i != len; ++i)
                {
                    args[i] = InternalGetNullable(input[i]);
                }

                result = tokenCache[nonTerm] = new SppfNode(production.Index, Loc.Unknown, args);
            }

            return result;
        }

        public SppfNode GetDefault(int nonTerm, IStackLookback<SppfNode> stackLookback)
        {
            return tokenCache[nonTerm];
        }
    }
}
