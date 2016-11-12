using IronText.Algorithm;
using IronText.Automata;
using IronText.Runtime;
using System.Collections.Generic;
using System.Linq;

namespace IronText.Automata
{
    class NullableFirstTables
    {
        private BitSetType      tokenSet;
        private MutableIntSet[] firsts;
        private bool[]          isNullable;

        protected IBuildtimeGrammar grammar;

        public NullableFirstTables(IBuildtimeGrammar grammar, TokenSetProvider tokenSetProvider)
        {
            this.grammar    = grammar;
            this.tokenSet   = tokenSetProvider.TokenSet;
            int count       = grammar.SymbolCount;
            this.firsts     = new MutableIntSet[count];
            this.isNullable = new bool[count];

            Build();
        }

        public bool[] TokenToNullable { get { return isNullable; } }

        private void Build()
        {
            int count  = grammar.SymbolCount;

            for (int i = 0; i != count; ++i)
            {
                firsts[i] = tokenSet.Mutable();
                if (grammar.IsTerminal(i))
                {
                    firsts[i].Add(i);
                }
            }

            var recursiveProds = new List<BuildtimeProduction>();

            // Init FIRST using rules without recursion in first part
            foreach (var prod in grammar.Productions)
            {
                if (prod.Input.Length == 0)
                {
                    firsts[prod.Outcome].Add(PredefinedTokens.Epsilon);
                }
                else if (grammar.IsTerminal(prod.Input[0]))
                {
                    firsts[prod.Outcome].Add(prod.Input[0]);
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
                    if (InternalAddFirsts(prod.Input, firsts[prod.Outcome]))
                    {
                        changed = true;
                    }
                }
            }
            while (changed);

            for (int i = 0; i != count; ++i)
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
        /// <param name="tailFirsts"></param>
        /// <returns><c>true</c> if chain is nullable, <c>false</c> otherwise</returns>
        public int FillFirsts(
            IEnumerable<int> tokenChain,
            IntSet           tailFirsts,
            MutableIntSet    output)
        {
            int countBefore = output.Count;

            bool nullable = true;

            foreach (int token in tokenChain)
            {
                output.AddAll(firsts[token]);
                if (!isNullable[token])
                {
                    nullable = false;
                    break;
                }
            }

            if (nullable)
            {
                output.AddAll(tailFirsts);
            }

            return output.Count - countBefore;
        }
    }
}
