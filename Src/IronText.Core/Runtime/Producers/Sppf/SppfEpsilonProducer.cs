using IronText.Logging;
using System.Diagnostics;
using System.Linq;

namespace IronText.Runtime
{
    class SppfEpsilonProducer
    {
        private readonly RuntimeGrammar grammar;
        private SppfNode[] tokenCache;
        private SppfNode[] ruleCache;
        private int[] ruleOffsetInCache;
        private int[] ruleEndOffsetInCache;

        public SppfEpsilonProducer(RuntimeGrammar grammar)
        {
            this.grammar = grammar;
            BuildCache();
        }

        private void BuildCache()
        {
            int tokenCount = grammar.LastToken;
            tokenCache = new SppfNode[tokenCount];

            foreach (int token in grammar.EnumerateTokens())
            {
                if (grammar.IsNullable(token))
                {
                    tokenCache[token] = InternalGetNullable(token);
                }
            }

            ruleOffsetInCache    = new int[grammar.LastProductionIndex];
            ruleEndOffsetInCache = new int[grammar.LastProductionIndex];

            int nullableCount = 0;
            foreach (var prod in grammar.Productions)
            {
                int i = prod.InputTokens.Length;
                while (i != 0)
                {
                    int token = prod.InputTokens[--i];
                    if (tokenCache[token] == null)
                    {
                        break;
                    }

                    ++nullableCount;
                }

                ruleEndOffsetInCache[prod.Index] = nullableCount;
                ruleOffsetInCache[prod.Index] = nullableCount - prod.InputTokens.Length;
            }

            this.ruleCache = new SppfNode[nullableCount];

            foreach (var prod in grammar.Productions)
            {
                int endOffset = ruleOffsetInCache[prod.Index] + prod.InputTokens.Length;
                int i = prod.InputTokens.Length;
                while (i != 0)
                {
                    int token = prod.InputTokens[--i];
                    if (tokenCache[token] == null)
                    {
                        break;
                    }

                    ruleCache[--endOffset] = tokenCache[token];
                }
            }
        }

        private SppfNode InternalGetNullable(int nonTerm)
        {
            Debug.Assert(grammar.IsNullable(nonTerm));

            var production = 
              (from r in grammar.GetProductions(nonTerm)
               where r.InputTokens.All(grammar.IsNullable)
               orderby r.InputTokens.Length ascending
               select r)
               .First();

            var args = new SppfNode[production.InputTokens.Length];
            for (int i = 0; i != args.Length; ++i)
            {
                args[i] = InternalGetNullable(production.InputTokens[i]);
            }

            return new SppfNode(production.Index, Loc.Unknown, args);
        }

        public void FillEpsilonSuffix(int prodId, int prefixSize, SppfNode[] dest, int destIndex, IStackLookback<SppfNode> stackLookback)
        {
            int i   = ruleOffsetInCache[prodId] + prefixSize;
            int end = ruleEndOffsetInCache[prodId];

            while (i != end)
            {
                dest[destIndex++] = ruleCache[i++];
            }
        }

        public SppfNode GetDefault(int nonTerm, IStackLookback<SppfNode> stackLookback)
        {
            return tokenCache[nonTerm];
        }
    }
}
