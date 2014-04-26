using System;
using System.IO;
using IronText.Collections;
using IronText.Reflection.Reporting;

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
            Options     = RuntimeOptions.Default;

            Productions = new ProductionCollection(this);
            Symbols     = new SymbolCollection(this);
            Matchers    = new MatcherCollection(this);
            Mergers     = new MergerCollection(this);
            Contexts    = new SemanticContextCollection(this);
            Reports     = new ReportCollection();
            GlobalContextProvider = new SemanticContextProvider { Owner = this.Contexts };
            Joint       = new Joint();

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

            var startStub = new Symbol("$start-stub");
            AugmentedProduction = Productions.Define((Symbol)Symbols[PredefinedTokens.AugmentedStart], new Symbol[] { startStub });
        }

        public RuntimeOptions Options { get; set; }

        public Joint Joint { get; private set; }

        public Symbol Start
        {
            get { return AugmentedProduction.Pattern[0]; }
            set { AugmentedProduction.SetAt(0, value); }
        }

        internal Production         AugmentedProduction { get; private set; }

        public SemanticContextProvider      GlobalContextProvider { get; private set; }

        public SymbolCollection     Symbols             { get; private set; }

        public ProductionCollection Productions         { get; private set; }

        public MatcherCollection    Matchers            { get; private set; }

        public MergerCollection     Mergers             { get; private set; }

        public SemanticContextCollection Contexts       { get; private set; }

        public ReportCollection     Reports             { get; private set; }

        public override string ToString()
        {
            var writerType = Type.GetType("IronText.Reflection.DefaultTextGrammarWriter, IronText.Compiler");
            if (writerType == null)
            {
                return "IronText.Reflection.Grammar";
            }

            var writer = (IGrammarTextWriter)Activator.CreateInstance(writerType);

            using (var output = new StringWriter())
            {
                writer.Write(this, output);
                return output.ToString();
            }
        }
    }
}
