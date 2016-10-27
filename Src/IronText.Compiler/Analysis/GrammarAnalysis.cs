using System.Collections.Generic;
using System.Linq;
using IronText.Algorithm;
using IronText.Reflection;
using IronText.Runtime;

namespace IronText.Compiler.Analysis
{
    /// <summary>
    /// Exposes <see cref="Grammar" /> information to the compile side
    /// </summary>
    sealed class GrammarAnalysis
    {
        private readonly Grammar grammar;

        public GrammarAnalysis(Grammar grammar)
        {
            this.grammar  = grammar;
        }

        public int SymbolCount => grammar.Symbols.Count;

        public string GetTokenName(int token) => grammar.Symbols[token].Name;

        public RuntimeProduction AugmentedProduction =>
            grammar.AugmentedProduction.ToRuntime();

        public IEnumerable<RuntimeProduction> GetProductions(int leftToken) =>
            grammar.Symbols[leftToken].Productions.Select(ProductionExtensions.ToRuntime);

        public RuntimeProduction GetProduction(int index) =>
            grammar.Productions[index].ToRuntime();

        public Precedence GetTermPrecedence(int token) =>
            grammar.Symbols[token].Precedence;

        public Precedence GetProductionPrecedence(int prodId) =>
            grammar.Productions[prodId].EffectivePrecedence;
    }
}
