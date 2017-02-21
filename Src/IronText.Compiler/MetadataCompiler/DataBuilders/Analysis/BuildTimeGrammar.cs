using System.Collections.Generic;
using System.Linq;
using IronText.Reflection;
using IronText.Automata;

namespace IronText.MetadataCompiler.Analysis
{
    /// <summary>
    /// Exposes <see cref="Grammar" /> information to the compile side
    /// </summary>
    sealed class BuildtimeGrammar : IBuildtimeGrammar
    {
        private readonly Grammar grammar;

        public BuildtimeGrammar(Grammar grammar)
        {
            this.grammar  = grammar;
        }

        public int SymbolCount => grammar.Symbols.AllCount;

        public IEnumerable<BuildtimeProduction> Productions =>
            grammar.Productions.All.Select(ProductionExtensions.ToBuildtime);

        public string GetTokenName(int token) => grammar.Symbols[token].Name;

        public BuildtimeProduction AugmentedProduction =>
            grammar.AugmentedProduction.ToBuildtime();

        public IEnumerable<BuildtimeProduction> GetProductions(int leftToken) =>
            grammar.Symbols[leftToken].Productions.Select(ProductionExtensions.ToBuildtime);

        public BuildtimeProduction GetProduction(int index) =>
            grammar.Productions[index].ToBuildtime();

        public bool IsTerminal(int token) =>
            grammar.Symbols[token].IsTerminal;

        public Precedence GetTermPrecedence(int token) =>
            grammar.Symbols[token].Precedence;

        public Precedence GetProductionPrecedence(int prodId) =>
            grammar.Productions[prodId].EffectivePrecedence;
    }
}
