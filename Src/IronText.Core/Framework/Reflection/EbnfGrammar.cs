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
        internal const int NoToken             = -1;
        internal const int EpsilonToken        = 0;
        internal const int PropogatedToken     = 1;

        public const int   ReservedTokenCount = 2;

        public const int AugmentedStart        = 2;
        public const int Eoi                   = 3;
        public const int Error                 = 4;
        public const int PredefinedTokenCount  = 5;

        private readonly ProductionCollection       productions;
        private readonly ProductionActionCollection productionActions;
        private readonly SymbolCollection           symbols;

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

            AugmentedProduction = Productions.Add(AugmentedStart, new[] { NoToken });
        }

        public SymbolCollection Symbols { get { return symbols; } }

        public ProductionCollection Productions { get { return productions; } }

        public Production AugmentedProduction { get; private set; }

        public int StartToken
        {
            get { return AugmentedProduction.Pattern[0]; }
            set { AugmentedProduction.Pattern[0] = value; }
        }

        public Symbol Start
        {
            get 
            {
                if (StartToken > 0)
                {
                    return (Symbol)symbols[StartToken];
                }

                return null;
            }
        }

        public bool IsStartProduction(int prodId)
        {
            return Productions[prodId].Outcome == StartToken;
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
                .AppendFormat("Start Token: {0}", Start == null ? "<undefined>" : Start.Name)
                .AppendLine()
                .Append("Rules:")
                .AppendLine();
            foreach (var prod in Productions)
            {
                output
                    .AppendFormat(
                        "{0:D2}: {1} -> {2}",
                        prod.Index,
                        prod.OutcomeSymbol.Name,
                        string.Join(" ", from s in prod.PatternSymbols select s.Name))
                    .AppendLine();
            }

            return output.ToString();
        }
    }
}
