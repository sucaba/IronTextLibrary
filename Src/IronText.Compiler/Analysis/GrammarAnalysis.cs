using System.Collections.Generic;
using System.Linq;
using IronText.Algorithm;
using IronText.Reflection;
using IronText.Runtime;

namespace IronText.Compiler.Analysis
{
    /// <summary>
    /// Prebuilds various tables related to <see cref="IronText.Framework.BnfGrammar"/>
    /// </summary>
    sealed class GrammarAnalysis
    {
        private readonly Grammar grammar;
        private readonly IBuildtimeNullableFirstTables tables;
        private int[]    tokenComplexity;

        public GrammarAnalysis(Grammar grammar)
        {
            this.grammar         = grammar;
            this.tables          = new NullableFirstTables(grammar);
            this.tokenComplexity = BuildTokenComplexity(grammar);
        }

        /// <summary>
        /// Fewer values are less dependent to higher values 
        /// Relation of values is non-determined for two mutally 
        /// dependent non-terms.
        /// </summary>
        public int[] GetTokenComplexity() { return tokenComplexity; }

        public string GetTokenName(int token)
        {
            return grammar.Symbols[token].Name;
        }

        public bool IsTerminal(int token)
        {
            return grammar.Symbols[token].IsTerminal;
        }

        public IEnumerable<ProdItem> GetProductions(int leftToken)
        {
            return grammar.Symbols[leftToken].Productions.Select(ToRt);
        }

        public int SymbolCount
        {
            get {  return grammar.Symbols.IndexCount; }
        }

        public Precedence GetTermPrecedence(int token)
        {
            return grammar.Symbols[token].Precedence;
        }

        public ProdItem AugmentedProduction
        {
            get { return ToRt(grammar.AugmentedProduction); }
        }

        public Precedence GetProductionPrecedence(int prodId)
        {
            return grammar.Productions[prodId].EffectivePrecedence;
        }

        public bool IsStartProduction(int prodId)
        {
            return grammar.Productions[prodId].IsStart;
        }

        public BitSetType TokenSet
        {
            get { return tables.TokenSet; }
        }

        public IEnumerable<AmbTokenInfo> AmbiguousSymbols
        {
            get { return grammar.Symbols.OfType<AmbiguousSymbol>().Select(ToRt); }
        }

        public void AddFirst(DotItem item, MutableIntSet output)
        {
            bool isNullable = tables.AddFirst(item.GetPattern(), item.Position, output);

            if (isNullable)
            {
                output.AddAll(item.LA);
            }
        }

        public bool HasFirst(DotItem item, int token)
        {
            return tables.HasFirst(item.GetPattern(), item.Position, token);
        }

        public bool IsTailNullable(DotItem item)
        {
            return tables.IsTailNullable(item.GetPattern(), item.Position);
        }

        private ProdItem ToRt(Production production)
        {
            return new ProdItem(
                production.Index,
                production.Outcome.Index,
                production.Pattern.Select(sym => sym.Index));
        }

        private AmbTokenInfo ToRt(AmbiguousSymbol symbol)
        {
            return new AmbTokenInfo(
                symbol.Index,
                symbol.MainToken,
                symbol.Tokens);
        }

        private static int[] BuildTokenComplexity(Grammar grammar)
        {
            var result = Enumerable.Repeat(-1, grammar.Symbols.IndexCount).ToArray();
            var sortedTokens = Graph.TopologicalSort(
                                new [] { PredefinedTokens.AugmentedStart },
                                t => GetDependantTokens(grammar, t))
                                .ToArray();
            for (int i = 0; i != sortedTokens.Length; ++i)
            {
                result[sortedTokens[i]] = i;
            }

            return result;
        }

        private static IEnumerable<int> GetDependantTokens(Grammar grammar, int token)
        {
            return grammar.Symbols[token].Productions.SelectMany(rule => rule.PatternTokens);
        }
    }
}
