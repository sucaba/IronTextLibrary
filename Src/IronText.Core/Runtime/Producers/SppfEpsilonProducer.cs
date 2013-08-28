namespace IronText.Framework
{
    class SppfEpsilonProducer
    {
        private readonly BnfGrammar grammar;
        private SppfNode[] tokenCache;
        private SppfNode[] ruleCache;
        private int[] ruleOffsetInCache;
        private int[] ruleEndOffsetInCache;

        public SppfEpsilonProducer(BnfGrammar grammar)
        {
            this.grammar = grammar;
            BuildCache();
        }

        private void BuildCache()
        {
            int tokenCount = grammar.TokenCount;
            tokenCache = new SppfNode[tokenCount];

            for (int token = 0; token != tokenCount; ++token)
            {
                if (grammar.IsNullable(token))
                {
                    tokenCache[token] = new SppfNode(token, null, Loc.Unknown);
                }
            }

            ruleOffsetInCache = new int[grammar.Rules.Count];
            ruleEndOffsetInCache = new int[grammar.Rules.Count];

            int nullableCount = 0;
            foreach (var rule in grammar.Rules)
            {
                int i = rule.Parts.Length;
                while (i != 0)
                {
                    int token = rule.Parts[--i];
                    if (tokenCache[token] == null)
                    {
                        break;
                    }

                    ++nullableCount;
                }

                ruleEndOffsetInCache[rule.Id] = nullableCount;
                ruleOffsetInCache[rule.Id] = nullableCount - rule.Parts.Length;
            }

            this.ruleCache = new SppfNode[nullableCount];

            foreach (var rule in grammar.Rules)
            {
                int endOffset = ruleOffsetInCache[rule.Id] + rule.Parts.Length;
                int i = rule.Parts.Length;
                while (i != 0)
                {
                    int token = rule.Parts[--i];
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

        public SppfNode GetEpsilonNonTerm(int nonTerm, IStackLookback<SppfNode> stackLookback)
        {
            return tokenCache[nonTerm];
        }
    }
}
