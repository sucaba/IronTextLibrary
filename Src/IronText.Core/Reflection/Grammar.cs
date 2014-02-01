using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using IronText.Algorithm;
using IronText.Collections;

namespace IronText.Reflection
{
    /// <summary>
    /// TODO:
    /// - Predefined entities should not be deletable.
    /// - Organize API to avoid IsPredefined checks.
    /// </summary>
    public sealed class Grammar : ISharedGrammarEntities
    {
        public const string UnnamedTokenName = "<unnamed token>";
        public const string UnknownTokenName = "<unknown token>";

        public Grammar()
        {
            Productions        = new ProductionCollection(this);
            Symbols            = new SymbolCollection(this);
            Conditions         = new ConditionCollection(this);
            Matchers           = new MatcherCollection(this);
            Mergers            = new MergerCollection(this);
            ProductionContexts = new ProductionContextCollection(this);

            for (int i = PredefinedTokens.Count; i != 0; --i)
            {
                Symbols.Add(null); // stub
            }

            Symbols[PredefinedTokens.Propagated]      = new Symbol("#");
            Symbols[PredefinedTokens.Epsilon]         = new Symbol("$eps");
            Symbols[PredefinedTokens.AugmentedStart]  = new Symbol("$start");
            Symbols[PredefinedTokens.Eoi]             = new Symbol("$")
                                          { 
                                              Categories = SymbolCategory.DoNotInsert 
                                                         | SymbolCategory.DoNotDelete 
                                          };
            Symbols[PredefinedTokens.Error]           = new Symbol("$error");

            AugmentedProduction = Productions.Define((Symbol)Symbols[PredefinedTokens.AugmentedStart], new Symbol[] { null });
        }

        public Production           AugmentedProduction { get; private set; }

        public SymbolCollection     Symbols             { get; private set; }

        public ProductionCollection Productions         { get; private set; }

        public MatcherCollection    Matchers            { get; private set; }

        public ConditionCollection  Conditions          { get; private set; }

        public MergerCollection     Mergers             { get; private set; }

        public ProductionContextCollection ProductionContexts  { get; private set; }

        public Symbol Start
        {
            get { return AugmentedProduction.Pattern[0]; }
            set { AugmentedProduction.SetAt(0, value); }
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
                .Append("Productions:")
                .AppendLine();
            foreach (var prod in Productions)
            {
                output
                    .AppendFormat(
                        "{0:D2}: {1} -> {2}",
                        prod.Index,
                        prod.Outcome.Name,
                        string.Join(" ", from s in prod.Pattern select s == null ? "<none>" : s.Name))
                    .AppendLine();
            }

            return output.ToString();
        }
    }
}
