using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using IronText.Algorithm;
using IronText.Framework.Collections;

namespace IronText.Framework.Reflection
{
    public sealed class EbnfGrammar : IEbnfContext
    {
        public const string UnnamedTokenName = "<unnamed token>";
        public const string UnknownTokenName = "<unknown token>";

        // Predefined tokens
        public const int NoToken               = -1;
        private const int EpsilonToken         = 0;
        public const int PropogatedToken       = 1;
        public const int AugmentedStart        = 2;
        public const int Eoi                   = 3;
        public const int Error                 = 4;
        public const int PredefinedTokenCount  = 5;

        // Special Tokens
        private const int SpecialTokenCount = 2;
        // Token IDs without TokenInfo

        // pending token count
        private BitSetType tokenSet;
        private readonly int AugmentedProductionIndex;

        private readonly ProductionCollection       productions;
        private readonly ProductionActionCollection productionActions;
        private readonly SymbolCollection           symbols;

        private MutableIntSet[] first;

        private bool[] isNullable;
        private bool   frozen;

        public EbnfGrammar()
        {
            productions       = new ProductionCollection(this);
            productionActions = new ProductionActionCollection(this);
            symbols           = new SymbolCollection(this);

            for (int i = PredefinedTokenCount; i != 0; --i)
            {
                Symbols.Add(new Symbol("")); // stub
            }

            Symbols[PropogatedToken] = new Symbol("#");
            Symbols[EpsilonToken]    = new Symbol("$eps");
            Symbols[AugmentedStart]  = new Symbol("$start");
            Symbols[Eoi]             = new Symbol("$")
                                          { 
                                              Categories = 
                                                         TokenCategory.DoNotInsert 
                                                         | TokenCategory.DoNotDelete 
                                          };
            Symbols[Error]           = new Symbol("$error");

            AugmentedProductionIndex = Productions.Add(AugmentedStart, new[] { -1 }).Index;
        }

        public SymbolCollection Symbols { get { return symbols; } }

        public ProductionCollection Productions { get { return productions; } }

        public BitSetType TokenSet 
        { 
            get 
            {
                Debug.Assert(frozen);
                return this.tokenSet; 
            } 
        }

        public int MaxRuleSize { get; private set; }

        public Production AugmentedProduction { get { return Productions[AugmentedProductionIndex];  } }

        public int? StartToken
        {
            get 
            { 
                int result = this.Productions[AugmentedProductionIndex].Pattern[0];
                return result < 0 ? null : (int?)result;
            }

            set { this.Productions[AugmentedProductionIndex].Pattern[0] = value.HasValue ? value.Value : -1; }
        }

        public Symbol Start
        {
            get 
            {
                int? token = StartToken;
                if (token.HasValue)
                {
                    return (Symbol)symbols[token.Value];
                }

                return null;
            }
        }

        public int SymbolCount { get { return Symbols.Count; } }

        public IEnumerable<AmbiguousSymbol> AmbiguousSymbols { get { return symbols.OfType<AmbiguousSymbol>(); } }

        public void Freeze()
        {
            this.frozen = true;
            this.tokenSet = new BitSetType(SymbolCount);

            EnsureFirsts();

            this.MaxRuleSize = Productions.Select(r => r.Pattern.Length).Max();
        }

        public bool IsStartProduction(int ruleId)
        {
            return Productions[ruleId].Outcome == AugmentedProduction.Pattern[0];
        }

        public bool IsBeacon(int token)
        {
            if (token >= Symbols.Count)
            {
                return false;
            }

            return (Symbols[token].Categories & TokenCategory.Beacon) != 0;
        }

        public bool IsDontInsert(int token)
        {
            if (token >= Symbols.Count)
            {
                return false;
            }

            return (Symbols[token].Categories & TokenCategory.DoNotInsert) != 0;
        }

        public bool IsDontDelete(int token)
        {
            if (token >= Symbols.Count)
            {
                return false;
            }

            return (Symbols[token].Categories & TokenCategory.DoNotDelete) != 0;
        }

        public bool IsTerminal(int token) { return Symbols[token].IsTerminal; }

        public bool IsNonTerm(int token) { return !Symbols[token].IsTerminal; }

        public TokenCategory GetTokenCategories(int token) { return Symbols[token].Categories; }

        internal bool IsExternal(int token) { return (Symbols[token].Categories & TokenCategory.External) != 0; }

        public bool IsNullable(int token) { return isNullable[token]; }

        public bool IsPredefined(int token) { return 0 <= token && token < PredefinedTokenCount; }

        public IEnumerable<Production> GetProductions(int left) { return Symbols[left].Productions; }

        public string SymbolName(int token) 
        {
            if (token < 0)
            {
                return UnknownTokenName;
            }

            return Symbols[token].Name; 
        }

        public IEnumerable<int> EnumerateTokens()
        {
            return Symbols.Select(ti => ti.Index);
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

                output.AddAll(first[token]);
                if (!isNullable[token])
                {
                    result = false;
                    break;
                }

                ++startIndex;
            }

            return result;
        }

        public bool HasFirst(int[] tokenChain, int startIndex, int token)
        {
            while (startIndex != tokenChain.Length)
            {
                int t = tokenChain[startIndex];

                if (first[t].Contains(token))
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

        internal int FirstNonNullableCount(IEnumerable<int> tokens)
        {
            return TrimRightNullable(tokens).Count();
        }

        internal IEnumerable<int> TrimRightNullable(IEnumerable<int> tokens)
        {
            return tokens.Reverse().SkipWhile(IsNullable).Reverse();
        }

        private void EnsureFirsts()
        {
            if (this.first == null)
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
            this.first     = new MutableIntSet[count];
            this.isNullable = new bool[count];

            for (int i = 0; i != count; ++i)
            {
                first[i] = tokenSet.Mutable();
                if (Symbols[i].IsTerminal)
                {
                    first[i].Add(i);
                }
            }

            var recursiveProds = new List<Production>();

            // Init FIRST using rules without recursion in first part
            foreach (var prod in Productions)
            {
                if (prod.Pattern.Length == 0)
                {
                    first[prod.Outcome].Add(EpsilonToken);
                }
                else if (Symbols[prod.Pattern[0]].IsTerminal)
                {
                    first[prod.Outcome].Add(prod.Pattern[0]);
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
                    if (InternalAddFirsts(prod.Pattern, first[prod.Outcome]))
                    {
                        changed = true;
                    }
                }
            }
            while (changed);

            for (int i = 0; i != count; ++i)
            {
                bool hasEpsilon = first[i].Contains(EpsilonToken);
                if (hasEpsilon)
                {
                    isNullable[i] = hasEpsilon;
                    first[i].Remove(EpsilonToken);
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
                foreach (var f in first[item].ToArray())
                {
                    if (f == EpsilonToken)
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

            if (nullable && !result.Contains(EpsilonToken))
            {
                result.Add(EpsilonToken);
                changed = true;
            }

            return changed;
        }

        public override string ToString()
        {
            var output = new StringBuilder();
            output
                .Append("Terminals: ")
                .Append(string.Join(" ", (from s in Symbols where s.IsTerminal select s.Name)))
                .AppendLine()
                .Append("Non-Terminals: ")
                .Append(string.Join(" ", (from s in Symbols where !s.IsTerminal select s.Name)))
                .AppendLine()
                .AppendFormat("Start Token: {0}", StartToken == null ? "<undefined>" : Start.Name)
                .AppendLine()
                .Append("Rules:")
                .AppendLine();
            foreach (var rule in Productions)
            {
                output.AppendFormat("{0:D2}: {1} -> {2}", rule.Index, SymbolName(rule.Outcome), string.Join(" ", rule.Pattern.Select(SymbolName))).AppendLine();
            }

            return output.ToString();
        }
    }
}
