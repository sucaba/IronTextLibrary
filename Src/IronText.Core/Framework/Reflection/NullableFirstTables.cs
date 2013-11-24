using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using IronText.Algorithm;

namespace IronText.Framework.Reflection
{
    public abstract class NullableFirstTables
    {
        private BitSetType tokenSet;
        private MutableIntSet[] First;
        private bool[] IsTokenNullable;
        private bool   frozen;

        protected EbnfGrammar grammar;

        /*
        public NullableFirstTables(EbnfGrammar grammar)
        {
            this.grammar = grammar;
        }
        */
        public abstract int SymbolCount { get; }

        public BitSetType TokenSet 
        { 
            get 
            {
                Debug.Assert(frozen);
                return this.tokenSet; 
            } 
        }

        public bool IsNullable(int token) { return IsTokenNullable[token]; }

        private void EnsureFirsts()
        {
            if (this.First == null)
            {
                BuildFirstFollowing();
            }
        }

        private void BuildFirstFollowing()
        {
            BuildFirst();
        }

        private void BuildFirst()
        {
            int count = SymbolCount;
            this.First     = new MutableIntSet[count];
            this.IsTokenNullable = new bool[count];

            for (int i = 0; i != count; ++i)
            {
                First[i] = tokenSet.Mutable();
                if (grammar.Symbols[i].IsTerminal)
                {
                    First[i].Add(i);
                }
            }

            var recursiveProds = new List<Production>();

            // Init FIRST using rules without recursion in first part
            foreach (var prod in grammar.Productions)
            {
                if (prod.Pattern.Length == 0)
                {
                    First[prod.Outcome].Add(EbnfGrammar.EpsilonToken);
                }
                else if (grammar.Symbols[prod.Pattern[0]].IsTerminal)
                {
                    First[prod.Outcome].Add(prod.Pattern[0]);
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
                    if (InternalAddFirsts(prod.Pattern, First[prod.Outcome]))
                    {
                        changed = true;
                    }
                }
            }
            while (changed);

            for (int i = 0; i != count; ++i)
            {
                bool hasEpsilon = First[i].Contains(EbnfGrammar.EpsilonToken);
                if (hasEpsilon)
                {
                    IsTokenNullable[i] = hasEpsilon;
                    First[i].Remove(EbnfGrammar.EpsilonToken);
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
                foreach (var f in First[item].ToArray())
                {
                    if (f == EbnfGrammar.EpsilonToken)
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

            if (nullable && !result.Contains(EbnfGrammar.EpsilonToken))
            {
                result.Add(EbnfGrammar.EpsilonToken);
                changed = true;
            }

            return changed;
        }

        public bool HasFirst(int[] tokenChain, int startIndex, int token)
        {
            while (startIndex != tokenChain.Length)
            {
                int t = tokenChain[startIndex];

                if (First[t].Contains(token))
                {
                    return true;
                }

                if (!IsTokenNullable[t])
                {
                    return false;
                }

                ++startIndex;
            }

            return false;
        }

        public bool IsTailNullable(int[] tokens, int startIndex)
        {
            bool result = true;

            while (startIndex != tokens.Length)
            {
                if (!IsTokenNullable[tokens[startIndex]])
                {
                    result = false;
                    break;
                }

                ++startIndex;
            }

            return result;
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

                output.AddAll(First[token]);
                if (!IsTokenNullable[token])
                {
                    result = false;
                    break;
                }

                ++startIndex;
            }

            return result;
        }

        public void Freeze()
        {
            this.frozen = true;
            this.tokenSet = new BitSetType(SymbolCount);

            EnsureFirsts();
        }

    }
}
