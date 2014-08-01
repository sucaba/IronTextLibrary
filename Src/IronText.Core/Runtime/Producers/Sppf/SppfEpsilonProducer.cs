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
            int tokenCount = grammar.TokenCount;
            tokenCache = new SppfNode[tokenCount];

            foreach (int token in grammar.EnumerateTokens())
            {
                if (grammar.IsNullable(token))
                {
                    tokenCache[token] = InternalGetNullable(token);
                }
            }

            ruleOffsetInCache = new int[grammar.Productions.IndexCount];
            ruleEndOffsetInCache = new int[grammar.Productions.IndexCount];

            int nullableCount = 0;
            foreach (var rule in grammar.Productions)
            {
                int i = rule.PatternTokens.Length;
                while (i != 0)
                {
                    int token = rule.PatternTokens[--i];
                    if (tokenCache[token] == null)
                    {
                        break;
                    }

                    ++nullableCount;
                }

                ruleEndOffsetInCache[rule.Index] = nullableCount;
                ruleOffsetInCache[rule.Index] = nullableCount - rule.PatternTokens.Length;
            }

            this.ruleCache = new SppfNode[nullableCount];

            foreach (var rule in grammar.Productions)
            {
                int endOffset = ruleOffsetInCache[rule.Index] + rule.PatternTokens.Length;
                int i = rule.PatternTokens.Length;
                while (i != 0)
                {
                    int token = rule.PatternTokens[--i];
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
               where r.PatternTokens.All(grammar.IsNullable)
               orderby r.PatternTokens.Length ascending
               select r)
               .First();

            var args = new SppfNode[production.PatternTokens.Length];
            for (int i = 0; i != args.Length; ++i)
            {
                args[i] = InternalGetNullable(production.PatternTokens[i]);
            }

            return new SppfNode(production.Index, Loc.Unknown, args);
        }

        public void FillEpsilonSuffix(int ruleId, int prefixSize, SppfNode[] dest, int destIndex, IStackLookback<SppfNode> stackLookback)
        {
            int i   = ruleOffsetInCache[ruleId] + prefixSize;
            int end = ruleEndOffsetInCache[ruleId];

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
