using IronText.Reflection;

namespace IronText.Framework
{
    class SppfEpsilonProducer
    {
        private readonly RuntimeEbnfGrammar grammar;
        private SppfNode[] tokenCache;
        private SppfNode[] ruleCache;
        private int[] ruleOffsetInCache;
        private int[] ruleEndOffsetInCache;

        public SppfEpsilonProducer(RuntimeEbnfGrammar grammar)
        {
            this.grammar = grammar;
            BuildCache();
        }

        private void BuildCache()
        {
            int tokenCount = grammar.SymbolCount;
            tokenCache = new SppfNode[tokenCount];

            for (int token = 0; token != tokenCount; ++token)
            {
                if (grammar.IsNullable(token))
                {
                    tokenCache[token] = new SppfNode(token, null, Loc.Unknown, HLoc.Unknown);
                }
            }

            ruleOffsetInCache = new int[grammar.Productions.Count];
            ruleEndOffsetInCache = new int[grammar.Productions.Count];

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
