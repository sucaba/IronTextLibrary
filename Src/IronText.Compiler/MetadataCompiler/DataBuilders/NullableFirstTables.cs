using IronText.Algorithm;
using IronText.Reflection;
using IronText.Runtime;
using System.Collections.Generic;
using System.Linq;

namespace IronText.MetadataCompiler
{
    class NullableFirstTables
    {
        private BitSetType      tokenSet;
        private MutableIntSet[] firsts;
        private bool[]          isNullable;

        protected Grammar grammar;

        public NullableFirstTables(Grammar grammar)
        {
            this.grammar    = grammar;
            int count       = grammar.Symbols.Count;
            this.tokenSet   = new BitSetType(count);
            this.firsts     = new MutableIntSet[count];
            this.isNullable = new bool[count];

            Build();
        }

        public bool[] TokenToNullable { get { return isNullable; } }

        public BitSetType TokenSet 
        { 
            get { return this.tokenSet; } 
        }

        public bool IsNullable(int token) { return isNullable[token]; }

        private void Build()
        {
            int first = grammar.Symbols.StartIndex;
            int last  = grammar.Symbols.Count;
            this.firsts     = grammar.Symbols.CreateCompatibleArray<MutableIntSet>();
            this.isNullable = grammar.Symbols.CreateCompatibleArray<bool>(false);

            for (int i = first; i != last; ++i)
            {
                firsts[i] = tokenSet.Mutable();
                if (grammar.Symbols[i].IsTerminal)
                {
                    firsts[i].Add(i);
                }
            }

            var recursiveProds = new List<Production>();

            // Init FIRST using rules without recursion in first part
            foreach (var prod in grammar.Productions)
            {
                if (prod.Input.Length == 0)
                {
                    firsts[prod.OutcomeToken].Add(PredefinedTokens.Epsilon);
                }
                else if (prod.Input[0].IsTerminal)
                {
                    firsts[prod.OutcomeToken].Add(prod.Input[0].Index);
                }
                else
                {
                    recursiveProds.Add(prod);
                }
            }

            // Iterate until no more changes possible
            bool changed;
            do
            {
                changed = false;

                foreach (var prod in recursiveProds)
                {
                    if (InternalAddFirsts(prod.InputTokens, firsts[prod.Outcome.Index]))
                    {
                        changed = true;
                    }
                }
            }
            while (changed);

            for (int i = first; i != last; ++i)
            {
                bool hasEpsilon = firsts[i].Contains(PredefinedTokens.Epsilon);
                if (hasEpsilon)
                {
                    isNullable[i] = hasEpsilon;
                    firsts[i].Remove(PredefinedTokens.Epsilon);
                }
            }
        }

        // Fill FIRST set for the chain of tokens.
        // Returns true if anything was added, false otherwise.
        private bool InternalAddFirsts(IEnumerable<int> chain, MutableIntSet result)
        {
            bool changed = false;

            bool nullable = true;
            foreach (int item in chain)
            {
                bool itemNullable = false;
                foreach (var f in firsts[item].ToArray())
                {
                    if (f == PredefinedTokens.Epsilon)
                    {
                        itemNullable = true; // current part is nullable
                        continue;
                    }

                    if (!result.Contains(f))
                    {
                        result.Add(f);
                        changed = true;
                    }
                }

                if (!itemNullable)
                {
                    nullable = false;
                    break;
                }
            }

            if (nullable && !result.Contains(PredefinedTokens.Epsilon))
            {
                result.Add(PredefinedTokens.Epsilon);
                changed = true;
            }

            return changed;
        }

        /// <summary>
        /// Firsts set of the token chain
        /// </summary>
        /// <param name="tokenChain"></param>
        /// <param name="output"></param>
        /// <returns><c>true</c> if chain is nullable, <c>false</c> otherwise</returns>
        public bool AddFirst(int[] tokenChain, int startIndex, MutableIntSet output)
        {
            bool result = true;

            while (startIndex != tokenChain.Length)
            {
                int token = tokenChain[startIndex];

                output.AddAll(firsts[token]);
                if (!isNullable[token])
                {
                    result = false;
                    break;
                }

                ++startIndex;
            }

            return result;
        }
    }
}
