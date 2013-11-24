using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using IronText.Algorithm;

namespace IronText.Framework.Reflection
{
    internal interface IBuildtimeNullableFirstTables
    {
        BitSetType TokenSet { get; }


        bool AddFirst(int[] tokenChain, int startIndex, MutableIntSet output);
        bool HasFirst(int[] tokenChain, int startIndex, int token);
        bool IsTailNullable(int[] tokens, int startIndex);
    }

    internal interface IRuntimeNullableFirstTables
    {
        bool IsNullable(int token);
    }

    public class NullableFirstTables : IBuildtimeNullableFirstTables, IRuntimeNullableFirstTables
    {
        private BitSetType      tokenSet;
        private MutableIntSet[] firsts;
        private bool[]          isNullable;

        protected EbnfGrammar grammar;

        public NullableFirstTables(EbnfGrammar grammar)
        {
            this.grammar    = grammar;
            int count       = grammar.Symbols.Count;
            this.tokenSet   = new BitSetType(count);
            this.firsts     = new MutableIntSet[count];
            this.isNullable = new bool[count];
            Build();
        }

        public BitSetType TokenSet 
        { 
            get { return this.tokenSet; } 
        }

        public bool IsNullable(int token) { return isNullable[token]; }

        private void Build()
        {
            int count = grammar.Symbols.Count;
            this.firsts     = new MutableIntSet[count];
            this.isNullable = new bool[count];

            for (int i = 0; i != count; ++i)
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
                if (prod.Pattern.Length == 0)
                {
                    firsts[prod.Outcome].Add(EbnfGrammar.EpsilonToken);
                }
                else if (grammar.Symbols[prod.Pattern[0]].IsTerminal)
                {
                    firsts[prod.Outcome].Add(prod.Pattern[0]);
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
                    if (InternalAddFirsts(prod.Pattern, firsts[prod.Outcome]))
                    {
                        changed = true;
                    }
                }
            }
            while (changed);

            for (int i = 0; i != count; ++i)
            {
                bool hasEpsilon = firsts[i].Contains(EbnfGrammar.EpsilonToken);
                if (hasEpsilon)
                {
                    isNullable[i] = hasEpsilon;
                    firsts[i].Remove(EbnfGrammar.EpsilonToken);
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

                if (firsts[t].Contains(token))
                {
                    return true;
                }

                if (!isNullable[t])
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
                if (!isNullable[tokens[startIndex]])
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
