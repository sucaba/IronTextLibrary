using System;
using System.Linq;
using System.IO;
using IronText.Collections;
using IronText.Reflection.Reporting;
using System.Collections.Generic;
using System.Diagnostics;

namespace IronText.Reflection
{
    /// <summary>
    /// TODO:
    /// - Predefined entities should not be deletable.
    /// - Organize API to avoid IsPredefined checks.
    /// </summary>
    public sealed class Grammar : IGrammarScope
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
            Reports     = new ReportCollection();
            Globals     = new SemanticScope();
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
            AugmentedProduction = Productions.Add((Symbol)Symbols[PredefinedTokens.AugmentedStart], new Symbol[] { startStub }, null);
        }

        public RuntimeOptions Options { get; set; }

        public Joint Joint { get; private set; }

        public string StartName
        {
            get {  return Start == null ? null : Start.Name; }
            set {  Start = value == null ? null : Symbols.ByName(value, createMissing: true); }
        }

        public Symbol Start
        {
            get { return AugmentedProduction.Pattern[0]; }
            set { AugmentedProduction.SetAt(0, value); }
        }

        internal Production         AugmentedProduction { get; private set; }

        public SemanticScope        Globals             { get; private set; }

        public SymbolCollection     Symbols             { get; private set; }

        public ProductionCollection Productions         { get; private set; }

        public MatcherCollection    Matchers            { get; private set; }

        public MergerCollection     Mergers             { get; private set; }

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

        public void Inline()
        {
            var symbolsToInline = (from symbol in Symbols
                                   let asNonAmb = symbol as Symbol
                                   where CanInline(asNonAmb)
                                   select asNonAmb)
                                  .ToArray();

            foreach (var symbol in symbolsToInline)
            {
                Inline(symbol);
            }
        }

        private static bool CanInline(Symbol symbol)
        {
            if (symbol == null 
                || symbol.IsPredefined 
                || symbol.IsStart
                || symbol.HasSideEffects)
            {
                return false;
            }

            return symbol.Productions.Count == 1 
                && (symbol.Productions.All(p => p.Size <= 1)
                    || 
                    symbol.Productions.All(
                        p => p.Pattern.All(s => s.IsTerminal)));
        }

        public void Inline(Symbol symbol)
        {
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

                int pos = Array.IndexOf(prod.Pattern, symbol);
                if (pos >= 0)
                {
                    productionsToExtend.AddRange(Extend(prod, pos));
                }
            }
        }

        private IEnumerable<Production> GetProductionsHavingInput(Symbol symbol)
        {
            foreach (var prod in Productions)
            {
                if (prod.Pattern.Contains(symbol) && !prod.IsDeleted && prod.IsUsed)
                {
                    yield return prod;
                }
            }
        }

        public IEnumerable<Production> Extend(Production source, int position)
        {
            var result = new List<Production>();

            var symbol = source.Pattern[position];

            source.MarkDeleted();

            var producitonsToInline = symbol.Productions.ToArray();
            foreach (var inlinedProd in producitonsToInline)
            {
                var extended = new ProductionInliner(inlinedProd).Execute(source, position);
                Productions.Add(extended);
                result.Add(extended);

                if (!inlinedProd.IsUsed)
                {
                    inlinedProd.MarkDeleted();
                }
            }

            return result;
        }

        public Symbol Decompose(Symbol nonTerm, Func<Production,bool> criteria, string newSymbolName)
        {
            Symbol newSymbol = Symbols.Add(newSymbolName);
            foreach (var prod in nonTerm.Productions.ToArray())
            {
                if (criteria(prod))
                {
                    var newProd = Productions.Add(
                        new Production(
                            newSymbol,
                            prod.Components,
                            contextRef: prod.ContextRef,
                            flags: prod.Flags));
                    newProd.ExplicitPrecedence = prod.ExplicitPrecedence;

                    prod.MarkDeleted();
                }
            }

            Productions.Add(new Production(nonTerm, new [] { newSymbol }));

            return newSymbol;
        }

        public Symbol[] FindOptionalPatternSymbols()
        {
            return Symbols.OfType<Symbol>().Where(IsOptionalSymbol).ToArray();
        }

        public void InlineOptionalSymbols()
        {
            var symbolsToInline = FindOptionalPatternSymbols();
            foreach (var symbol in symbolsToInline)
            {
                Inline(symbol);
            }
        }

        private static bool IsOptionalSymbol(Symbol symbol)
        {
            if (symbol == null)
            {
                return false;
            }

            var prods = symbol.Productions;
            bool result = prods.Count == 2
                        && prods.Any(p => p.Pattern.Length == 0)
                        && prods.Any(p => p.Pattern.Length == 1);
            return result;
        }

        public void NullableToOpt()
        {
            var nullableSymbols = FindNullableSymbols();
            foreach (var symbol in nullableSymbols)
            {
                Decompose(symbol, IsNonNullable, symbol.Name + "nn");
            }
        }

        public Symbol[] FindNullableSymbols()
        {
            return Symbols.OfType<Symbol>().Where(IsNullable).ToArray();
        }

        private static bool IsNonNullable(Production production)
        {
            return !IsNullable(production);
        }

        private static bool IsNullable(Symbol symbol)
        {
            return symbol != null && symbol.Productions.Any(IsNullable);
        }

        private static bool IsNullable(Production production)
        {
            return production.Pattern.All(IsNullable);
        }
    }
}
