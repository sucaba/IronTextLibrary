﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using IronText.Algorithm;
using IronText.Collections;

namespace IronText.Framework.Reflection
{
    /// <summary>
    /// TODO:
    /// - Predefined entities should not be deletable.
    /// - Organize API to avoid IsPredefined checks.
    /// </summary>
    public sealed class EbnfGrammar : IEbnfContext
    {
        public const string UnnamedTokenName = "<unnamed token>";
        public const string UnknownTokenName = "<unknown token>";

        #region Reserved tokens

        internal const int NoToken             = -1;
        internal const int EpsilonToken        = 0;
        internal const int PropogatedToken     = 1;

        public const int ReservedTokenCount    = 2;

        #endregion Reserved tokens

        #region Predefined tokens

        public const int AugmentedStart        = 2;

        public const int Eoi                   = 3;

        public const int Error                 = 4;

        public const int PredefinedSymbolCount = 5;

        #endregion Predefined tokens

        private readonly ProductionCollection               productions;
        private readonly ProductionActionCollection         productionActions;
        private readonly SymbolCollection                   symbols;
        private readonly ScanConditionCollection            scanConditions;
        private readonly MergerCollection                   mergers;
        private readonly ScanProductionCollection           scanProductions;
        private readonly ProductionContextCollection         reductionContexts;
        private readonly ProductionContextProviderCollection reductionContextProviders;

        public EbnfGrammar()
        {
            productions       = new ProductionCollection(this);
            productionActions = new ProductionActionCollection(this);
            symbols           = new SymbolCollection(this);
            scanConditions    = new ScanConditionCollection(this);
            scanProductions   = new ScanProductionCollection(this);
            mergers           = new MergerCollection(this);
            reductionContexts = new ProductionContextCollection(this);
            reductionContextProviders = new ProductionContextProviderCollection(this);

            for (int i = PredefinedSymbolCount; i != 0; --i)
            {
                Symbols.Add(null); // stub
            }

            Symbols[PropogatedToken] = new Symbol("#");
            Symbols[EpsilonToken]    = new Symbol("$eps");
            Symbols[AugmentedStart]  = new Symbol("$start");
            Symbols[Eoi]             = new Symbol("$")
                                          { 
                                              Categories = SymbolCategory.DoNotInsert 
                                                         | SymbolCategory.DoNotDelete 
                                          };
            Symbols[Error]           = new Symbol("$error");

            AugmentedProduction = Productions.Define((Symbol)Symbols[AugmentedStart], new Symbol[] { null });
        }

        public Production                         AugmentedProduction       { get; private set; }

        public SymbolCollection                   Symbols                   { get { return symbols; } }

        public ProductionCollection               Productions               { get { return productions; } }

        public ScanProductionCollection           ScanProductions           { get { return scanProductions; } }

        public ScanConditionCollection            ScanConditions            { get { return scanConditions; } }

        public MergerCollection                   Mergers                   { get { return mergers; } }

        public ProductionContextCollection         ProductionContexts         { get { return reductionContexts; } }

        public ProductionContextProviderCollection ProductionContextProviders { get { return reductionContextProviders; } }

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
