using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using IronText.Algorithm;

namespace IronText.Framework.Reflection
{
    internal interface IRuntimeBnfGrammar
    {
        List<Production> Rules { get; }

        bool IsNullable(int token);

        IEnumerable<Production> GetProductions(int leftToken);
    }

    public interface IBuildtimeBnfGrammar
    {
        string SymbolName(int token);

        bool IsTerminal(int token);

        IEnumerable<Production> GetProductions(int leftToken);

        int SymbolCount { get; }

        int AmbSymbolCount { get; }

        Precedence GetTermPrecedence(int token);

        Production AugmentedProduction { get; }

        Precedence GetProductionPrecedence(int ruleId);

        bool IsStartProduction(int ruleId);

        BitSetType TokenSet { get; }

        IEnumerable<AmbiguousSymbol> AmbTokens { get; }

        bool AddFirst(int[] tokenChain, int startIndex, MutableIntSet output);

        bool HasFirst(int[] tokenChain, int startIndex, int token);

        bool IsTailNullable(int[] tokens, int startIndex);
    }

    public sealed class EbnfGrammar 
        : IRuntimeBnfGrammar
        , IBuildtimeBnfGrammar
    {
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
        private int allTokenCount = PredefinedTokenCount;
        private BitSetType tokenSet;
        private readonly List<Symbol> symbols;
        private readonly List<AmbiguousSymbol> ambTokens;
        private readonly int AugmentedStartRuleId;

        private MutableIntSet[] first;

        private bool[] isNullable;
        private bool frozen;

        public EbnfGrammar()
        {   
            Rules = new List<Production>();
            symbols = new List<Symbol>(PredefinedTokenCount);
            ambTokens = new List<AmbiguousSymbol>();
            for (int i = PredefinedTokenCount; i != 0; --i)
            {
                symbols.Add(null);
            }

            symbols[PropogatedToken] = new Symbol { Name = "#" };
            symbols[EpsilonToken]    = new Symbol { Name = "$eps" };
            symbols[AugmentedStart]  = new Symbol { Name = "$start" };
            symbols[Eoi]             = new Symbol 
                                          { 
                                              Name = "$",
                                              Categories = 
                                                         TokenCategory.DoNotInsert 
                                                         | TokenCategory.DoNotDelete 
                                          };
            symbols[Error]           = new Symbol { Name = "$error" };

            AugmentedStartRuleId = DefineRule(AugmentedStart, new[] { -1 });
        }

        public List<Production> Rules { get; private set; }

        public BitSetType TokenSet 
        { 
            get 
            {
                Debug.Assert(frozen);
                return this.tokenSet; 
            } 
        }

        public int MaxRuleSize { get; private set; }

        public Production AugmentedProduction { get { return Rules[AugmentedStartRuleId];  } }

        public int? StartToken
        {
            get 
            { 
                int result = this.Rules[AugmentedStartRuleId].Parts[0];
                return result < 0 ? null : (int?)result;
            }

            set { this.Rules[AugmentedStartRuleId].Parts[0] = value.HasValue ? value.Value : -1; }
        }

        public int SymbolCount { get { return symbols.Count; } }

        public int AmbSymbolCount { get { return ambTokens.Count; } }

        public IEnumerable<AmbiguousSymbol> AmbTokens { get { return ambTokens; } }

        public void Freeze()
        {
            this.frozen = true;
            this.tokenSet = new BitSetType(SymbolCount);

            EnsureFirsts();
            for (int i = PredefinedTokenCount; i != SymbolCount; ++i)
            {
                if (i == Error)
                {
                    symbols[Error].IsTerm = false;
                }
                else
                {
                    symbols[i].IsTerm = CalcIsTerm(i);
                }
            }

            symbols[Eoi].IsTerm = true;

            this.MaxRuleSize = Rules.Select(r => r.Parts.Length).Max();
        }

        public int DefineToken(string name, TokenCategory categories = TokenCategory.None)
        {
            Debug.Assert(!frozen);

            int result = InternalDefineToken(name, categories);
            if (null == StartToken)
            {
                StartToken = result;
            }

            return result;
        }

        public int DefineAmbToken(int mainToken, IEnumerable<int> tokens)
        {
            int result = allTokenCount++;
            var ambToken = new AmbiguousSymbol(result, mainToken, tokens);
            ambTokens.Add(ambToken);
            return result;
        }

        public bool IsStartProduction(int ruleId)
        {
            return Rules[ruleId].Left == AugmentedProduction.Parts[0];
        }

        internal bool IsBeacon(int token)
        {
            if (token >= symbols.Count)
            {
                return false;
            }

            return (symbols[token].Categories & TokenCategory.Beacon) != 0;
        }

        internal bool IsDontInsert(int token)
        {
            if (token >= symbols.Count)
            {
                return false;
            }

            return (symbols[token].Categories & TokenCategory.DoNotInsert) != 0;
        }

        internal bool IsDontDelete(int token)
        {
            if (token >= symbols.Count)
            {
                return false;
            }

            return (symbols[token].Categories & TokenCategory.DoNotDelete) != 0;
        }

        private int InternalDefineToken(string name, TokenCategory categories)
        {
            int result = allTokenCount++;
            symbols.Add(new Symbol { Id = result, Name = name ?? "token-" + result, Categories = categories });
            return result;
        }

        public bool IsTerminal(int token) { return symbols[token].IsTerm; }

        public bool IsNonTerm(int token) 
        {
            var ti = symbols[token];
            return !ti.IsTerm && (token >= PredefinedTokenCount);
        }

        internal TokenCategory GetTokenCategories(int token) { return symbols[token].Categories; }

        internal bool IsExternal(int token) { return (symbols[token].Categories & TokenCategory.External) != 0; }

        public bool IsNullable(int token) { return isNullable[token]; }

        public bool IsPredefined(int token) { return 0 <= token && token < PredefinedTokenCount; }

        private Production GetRule(int rule) { return this.Rules[rule]; }

        public IEnumerable<Production> GetProductions(int left) { return symbols[left].Productions; }

        private bool CalcIsTerm(int token)
        {
            bool result = !Rules.Any(rule => rule.Left == token);
            return result;
        }

        public string SymbolName(int token) 
        {
            if (token < 0)
            {
                return "<undefined token>";
            }

            return symbols[token].Name; 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="parts"></param>
        /// <returns>Rule ID or -1 if there is no such rule</returns>
        internal int FindRuleId(int left, int[] parts)
        {
            for (int i = 0; i != Rules.Count; ++i)
            {
                var rule = Rules[i];
                if (rule.Left == left
                    && rule.Parts.Length == parts.Length
                    && Enumerable.SequenceEqual(rule.Parts, parts))
                {
                    return i;
                }
            }

            return -1;
        }

        public int DefineRule(int left, int[] parts)
        {
            Debug.Assert(!frozen);

            if (IsExternal(left))
            {
                throw new InvalidOperationException(
                    "Unable to define rule for external token. This token should be represented by the external reciver logic.");
            }

            int result = this.Rules.Count;

            var rule = new Production
                {
                    Id = result,
                    Left = left,
                    Parts = parts,
                };

            this.Rules.Add(rule);

            symbols[left].Productions.Add(rule);
            
            return result;
        }

        // TODO: Optmize
        internal IEnumerable<Production> TokenRules(int token)
        {
            var result = this.Rules.Where(r => r.Left == token).ToArray();
            if (result.Length == 0)
            {
                throw new InvalidOperationException("Term token has no rules.");
            }

            return result;
        }

        public IEnumerable<int> EnumerateTokens()
        {
            return symbols.Select(ti => ti.Id);
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
                if (CalcIsTerm(i))
                {
                    first[i].Add(i);
                }
            }

            var recursiveRules = new List<Production>();

            // Init FIRST using rules without recursion in first part
            foreach (var rule in Rules)
            {
                if (rule.Parts.Length == 0)
                {
                    first[rule.Left].Add(EpsilonToken);
                }
                else if (CalcIsTerm(rule.Parts[0]))
                {
                    first[rule.Left].Add(rule.Parts[0]);
                }
                else
                {
                    recursiveRules.Add(rule);
                }
            }

            // Iterate until no more changes possible
            bool changed;
            do
            {
                changed = false;

                foreach (var rule in recursiveRules)
                {
                    if (InternalAddFirsts(rule.Parts, first[rule.Left]))
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

        public override bool Equals(object obj)
        {
            var casted = obj as EbnfGrammar;
            return Equals(casted);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Rules.Sum(rule => rule.GetHashCode())
                    + symbols.Sum(t => t.GetHashCode());
            }
        }

        public bool Equals(EbnfGrammar other)
        {
            return other != null
                && Enumerable.SequenceEqual(other.Rules, this.Rules)
                && Enumerable.SequenceEqual(other.symbols, this.symbols);
        }

        public override string ToString()
        {
            var output = new StringBuilder();
            output
                .Append("Terminals: ")
                .Append(string.Join(" ", EnumerateTokens().Where(IsTerminal).Select(SymbolName)))
                .AppendLine()
                .Append("Non-Terminals: ")
                .Append(string.Join(" ", EnumerateTokens().Where(t => !IsTerminal(t)).Select(SymbolName)))
                .AppendLine()
                .AppendFormat("Start Token: {0}", StartToken == null ? "<undefined>" : SymbolName(StartToken.Value))
                .AppendLine()
                .Append("Rules:")
                .AppendLine();
            foreach (var rule in Rules)
            {
                output.AppendFormat("{0:D2}: {1} -> {2}", rule.Id, SymbolName(rule.Left), string.Join(" ", rule.Parts.Select(SymbolName))).AppendLine();
            }

            return output.ToString();
        }


        public Precedence GetTermPrecedence(int token)
        {
            if (!IsTerminal(token))
            {
                throw new ArgumentException("Precedence is applicable only to terminals.", "token");
            }

            var info = this.symbols[token];
            return info.Precedence;
        }

        public void SetTermPrecedence(int token, Precedence precedence)
        {
            if (!CalcIsTerm(token))
            {
                throw new ArgumentException("Precedence is applicable only to terminals.", "token");
            }

            var info = this.symbols[token];
            info.Precedence = precedence;
        }

        public Precedence GetProductionPrecedence(int ruleId)
        {
            var rule = GetRule(ruleId);
            if (rule.Precedence != null)
            {
                return rule.Precedence;
            }

            int index = Array.FindLastIndex(rule.Parts, CalcIsTerm);
            return index < 0 ? null : GetTermPrecedence(rule.Parts[index]);
        }

        public void SetRulePrecedence(int ruleId, Precedence value)
        {
            var rule = GetRule(ruleId);
            rule.Precedence = value;
        }
    }
}
