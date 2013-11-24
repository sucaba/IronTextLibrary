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
    public sealed class EbnfGrammar : NullableFirstTables, IEbnfContext
    {
        public const string UnnamedTokenName = "<unnamed token>";
        public const string UnknownTokenName = "<unknown token>";

        // Predefined tokens
        public const int NoToken               = -1;
        internal const int EpsilonToken         = 0;
        public const int PropogatedToken       = 1;
        public const int AugmentedStart        = 2;
        public const int Eoi                   = 3;
        public const int Error                 = 4;
        public const int PredefinedTokenCount  = 5;

        // Special Tokens
        private const int SpecialTokenCount = 2;
        // Token IDs without TokenInfo

        private readonly int AugmentedProductionIndex;

        private readonly ProductionCollection       productions;
        private readonly ProductionActionCollection productionActions;
        private readonly SymbolCollection           symbols;

        public EbnfGrammar()
        {
            base.grammar = this;

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

        public override int SymbolCount { get { return Symbols.Count; } }

        public IEnumerable<AmbiguousSymbol> AmbiguousSymbols { get { return symbols.OfType<AmbiguousSymbol>(); } }

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

        public bool IsPredefined(int token) { return 0 <= token && token < PredefinedTokenCount; }

        public string SymbolName(int token) 
        {
            if (token < 0)
            {
                return UnknownTokenName;
            }

            return Symbols[token].Name; 
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
