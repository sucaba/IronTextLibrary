using IronText.Algorithm;
using IronText.Collections;
using IronText.DI;
using IronText.Reporting;
using IronText.Reflection.Validation;
using IronText.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IronText.Reflection
{
    /// <summary>
    /// TODO:
    /// - Predefined entities should not be deletable.
    /// - Organize API to avoid IsPredefined checks.
    /// </summary>
    [Serializable]
    public sealed class Grammar : IGrammarScope, IDependencyResolver
    {
        public const string UnnamedTokenName = "<unnamed token>";
        public const string UnknownTokenName = "<unknown token>";

        private const int AugStartProductionIndex = 0;

        [NonSerialized]
        private readonly Joint _joint = new Joint();

        [NonSerialized]
        private readonly ReportCollection _reports = new ReportCollection();

        public Grammar()
        {
            Options     = RuntimeOptions.Default;

            AugmentedStart = new Symbol("$start");
            Error          = new Symbol("$error");
            Eoi            = new Symbol("$")       { Categories = SymbolCategory.DoNotInsert | SymbolCategory.DoNotDelete };

            var startStub = new Symbol("$start-stub");
            AugmentedProduction = new Production(this.AugmentedStart, new [] { startStub });

            Symbols        = new SymbolCollection(this)
            {
                { new Symbol("$epsilon")  { IsPredefined = true },  PredefinedTokens.Epsilon        },
                { new Symbol("$propagated") { IsPredefined = true },  PredefinedTokens.Propagated     },
                { AugmentedStart,           PredefinedTokens.AugmentedStart },
                { Error,                    PredefinedTokens.Error          }, 
                { Eoi,                      PredefinedTokens.Eoi            }
            };

            Productions    = new ProductionCollection(this)
            {
                { AugmentedProduction, AugStartProductionIndex }
            };

            Matchers       = new MatcherCollection(this);
            Mergers        = new MergerCollection(this);
            Globals        = new SemanticScope();

            foreach (var symbol in new[] { AugmentedStart, Error, Eoi})
            {
                symbol.IsPredefined = true;
            }

            this.SymbolProperties    = new SymbolPropertyCollection(this);
            this.InheritedProperties = new InheritedPropertyCollection(this);
        }

        private IDependencyResolver DR { get {  return this; } }

        public RuntimeOptions Options { get; set; }

        public bool AreNonTermsBottomUpByDefault =>
            !Options.HasFlag(RuntimeOptions.ForceGeneric);

        public Joint Joint { get { return _joint; } }

        public string StartName
        {
            get {  return Start == null ? null : Start.Name; }
            set {  Start = value == null ? null : Symbols.ByName(value, createMissing: true); }
        }

        internal Symbol AugmentedStart { get; set; }

        private Symbol Eoi             {  get; set; }

        internal Symbol Error          {  get; private set; }

        public Symbol Start
        {
            get { return AugmentedProduction.Input[0]; }
            set { AugmentedProduction.SetAt(0, value); }
        }

        public Production           AugmentedProduction { get; private set; }

        public SemanticScope        Globals             { get; private set; }

        public SymbolCollection     Symbols             { get; private set; }

        public IEnumerable<Symbol> NonTerms => Symbols.Where(s => !s.IsTerminal);

        public ProductionCollection Productions         { get; private set; }

        public MatcherCollection    Matchers            { get; private set; }

        public MergerCollection     Mergers             { get; private set; }

        public ReportCollection     Reports             { get { return _reports; } }

        public SymbolPropertyCollection     SymbolProperties { get; private set; }

        public InheritedPropertyCollection  InheritedProperties { get; private set; }

        public void BuildIndexes()
        {
            Symbols.BuildIndexes();
            Productions.BuildIndexes();
            Matchers.BuildIndexes();
            Mergers.BuildIndexes();
            SymbolProperties.BuildIndexes();
            InheritedProperties.BuildIndexes();
        }

        public override string ToString()
        {
            var writer = DR.Get<IGrammarTextWriter>();

            using (var output = new StringWriter())
            {
                writer.Write(this, output);
                return output.ToString();
            }
        }

        object IDependencyResolver.Get(Type type)
        {
            if (type == typeof(IProductionResolver))
            {
                return new ProductionResolver(Symbols, Productions, DR.Get<IProductionTextMatcher>());
            }

            if (type == typeof(ISymbolTextMatcher) || type == typeof(IProductionTextMatcher) || type == typeof(IInjectedActionParameterTextMatcher))
            {
                return new GrammarElementMatcher();
            }

            if (type == typeof(IGrammarScope))
            {
                return this;
            }
            
            if (type == typeof(IGrammarTextWriter))
            {
                var writerType = Type.GetType("IronText.Reflection.DefaultTextGrammarWriter, IronText.Compiler");
                if (writerType == null)
                {
                    return "IronText.Reflection.Grammar";
                }

                return (IGrammarTextWriter)Activator.CreateInstance(writerType);
            }

            return null;
        }

        public void RequireImmutable()
        {
            Symbols.RequireIndexed();
        }
    }
}
