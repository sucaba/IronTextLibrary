using IronText.Algorithm;
using IronText.Collections;
using IronText.Reflection.Reporting;
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
            var writer = DR.Resolve<IGrammarTextWriter>();

            using (var output = new StringWriter())
            {
                writer.Write(this, output);
                return output.ToString();
            }
        }

        public void Inline()
        {
            InlineNonAlternateAliasSymbols();
        }

        public void InlineNonAlternateAliasSymbols()
        {
            var symbolsToInline = Symbols.Where(IsNonAlternateAliasSymbol).ToArray();
            foreach (var symbol in symbolsToInline)
            {
                Inline(symbol);
            }
        }

        public void InlineEmptyProductions()
        {
            ConvertNullableNonOptToOpt();
            RecursivelyEliminateEmptyProductions();
        }

        internal void RecursivelyEliminateEmptyProductions()
        {
            while (EliminateEmptyProductions())
            {
            }
        }

        internal bool EliminateEmptyProductions()
        {
            var old = Productions.DuplicateResolver;
            Productions.DuplicateResolver = ProductionDuplicateResolver.Instance;
            try
            {
                return InternalEliminateEmptyProductions();
            }
            finally
            {
                Productions.DuplicateResolver = old;
            }
        }

        private bool InternalEliminateEmptyProductions()
        {
            bool result = false;

            var nullableSymbols = FindNullableSymbols().ToArray();
            foreach (var symbol in nullableSymbols)
            {
                result = result || Inline(symbol);
            }

            return result;
        }

        private IEnumerable<Symbol> FindNullableSymbols()
        {
            var result = Symbols
                    .Where(s => s.Productions.Count != 0
                             && s.Productions.Any(p => p.Input.Length == 0)
                             && !s.IsRecursive);

            return result;
        }

        private static bool IsNonAlternateAliasSymbol(Symbol symbol)
        {
            if (symbol == null 
                || symbol.IsPredefined 
                || symbol.IsStart
                || symbol.HasSideEffects)
            {
                return false;
            }

            var result =
                symbol.Productions.Count == 1 
                && (symbol.Productions.All(p => p.InputLength <= 1)
                    || 
                    symbol.Productions.All(
                        p => p.Input.All(s => s.IsTerminal)));

            if (result)
            {
                result = !symbol.IsRecursive;
            }

            return result;
        }

        public bool Inline(Symbol symbol)
        {
            bool result = false;

            if (symbol == null)
            {
                throw new ArgumentNullException("symbol");
            }

            if (symbol.IsTerminal)
            {
                throw new ArgumentException("Cannot inline terminal symbol.", "symbol");
            }

            var productionsToExtend = GetProductionsHavingInput(symbol).ToList();

            for (int i = 0; i != productionsToExtend.Count; ++i)
            {
                var prod = productionsToExtend[i];

                int pos = Array.IndexOf(prod.Input, symbol);
                if (pos >= 0)
                {
                    productionsToExtend.AddRange(ExpandAt(prod, pos));
                    result = true;
                }
            }

            return result;
        }

        private IEnumerable<Production> GetProductionsHavingInput(Symbol symbol)
        {
            foreach (var prod in Productions)
            {
                if (prod.Input.Contains(symbol))
                {
                    yield return prod;
                }
            }
        }

        internal IEnumerable<Production> ExpandAt(Production source, int position)
        {
            var result = new List<Production>();

            var symbol = source.Input[position];

            SoftRemove(source);

            var producitonsToInline = symbol.Productions.ToArray();
            foreach (var inlinedProd in producitonsToInline)
            {
                var extended = new ProductionInliner(inlinedProd).Execute(source, position);
                Productions.Add(extended);
                result.Add(extended);

                if (!inlinedProd.IsUsed)
                {
                    SoftRemove(inlinedProd);
                }
            }

            return result;
        }

        private void SoftRemove(Production inlinedProd)
        {
            Productions.SoftRemove(inlinedProd);
        }

        public Symbol Decompose(Symbol nonTerm, Func<Production,bool> criteria, string newSymbolName)
        {
            Symbol newSymbol = Symbols.Add(newSymbolName);
            newSymbol.Joint.AddAll(nonTerm.Joint);

            foreach (var prod in nonTerm.Productions.ToArray())
            {
                if (criteria(prod))
                {
                    var newProd = Productions.Add(
                        new Production(
                            newSymbol,
                            prod.ChildComponents,
                            contextRef: prod.ContextRef,
                            flags: prod.Flags));
                    newProd.ExplicitPrecedence = prod.ExplicitPrecedence;

                    newProd.Joint.AddAll(prod.Joint);

                    SoftRemove(prod);
                }
            }

            Productions.Add(new Production(nonTerm, newSymbol));

            return newSymbol;
        }

        public Symbol[] FindOptionalPatternSymbols()
        {
            return Symbols.Where(SymbolTraits.IsOptionalSymbol).ToArray();
        }

        public bool InlineOptionalSymbols()
        {
            bool result = false;

            var symbolsToInline = FindOptionalPatternSymbols();
            foreach (var symbol in symbolsToInline)
            {
                Inline(symbol);
                result = true;
            }

            return result;
        }

        public void ConvertNullableNonOptToOpt()
        {
            var nullableSymbols = FindNullableNonOptSymbols();
            foreach (var symbol in nullableSymbols)
            {
                Decompose(symbol, prod => !prod.Input.All(nullableSymbols.Contains), symbol.Name + SymbolTraits.SomeSymbolSuffix);
            }
        }

        public IEnumerable<Symbol> FindNullableNonOptSymbols()
        {
            var result = Symbols
                   .Where(s => 
                       s.Productions.Any(p => p.Input.Length == 0)
                       && s.Productions.Any(p => p.Input.Length != 0)
                       && (s.Productions.Count != 2 
                          || s.Productions.Any(p => p.Input.Length > 1)));
            return result.ToArray();
        }

        object IDependencyResolver.Resolve(Type type)
        {
            if (type == typeof(IProductionResolver))
            {
                return new ProductionResolver(Symbols, Productions, DR.Resolve<IProductionTextMatcher>());
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
