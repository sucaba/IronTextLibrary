using System.Collections.Generic;
using System.Linq;
using IronText.Algorithm;
using IronText.Reflection;
using IronText.Runtime;
using System;
using IronText.Misc;
using IronText.MetadataCompiler;
using IronText.Compiler.Analysis;

namespace IronText.Compiler.Analysis
{
    /// <summary>
    /// Prebuilds various tables related to <see cref="IronText.Reflection.Grammar"/>
    /// </summary>
    sealed class GrammarAnalysis
    {
        private readonly Grammar grammar;
        private readonly IBuildtimeNullableFirstTables tables;
        private int[]    tokenComplexity;
        private AmbTokenInfo[] ambiguities;

        public GrammarAnalysis(Grammar grammar, AmbTokenInfo[] ambiguities)
        {
            this.grammar         = grammar;
            this.ambiguities     = ambiguities;
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

        public IEnumerable<RuntimeProduction> GetProductions(int leftToken)
        {
            return grammar.Symbols[leftToken].Productions.Select(ToRt);
        }

        public int TotalSymbolCount
        {
            get {  return grammar.Symbols.Count + ambiguities.Length; }
        }

        public Precedence GetTermPrecedence(int token)
        {
            return grammar.Symbols[token].Precedence;
        }

        public RuntimeProduction AugmentedProduction
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

        public IEnumerable<AmbTokenInfo> AmbiguousSymbols { get { return ambiguities; } }

        public void AddFirst(DotItem item, MutableIntSet output)
        {
            bool isNullable = tables.AddFirst(item.GetInputTokens(), item.Position, output);

            if (isNullable)
            {
                output.AddAll(item.LA);
            }
        }

        public bool HasFirst(DotItem item, int token)
        {
            return tables.HasFirst(item.GetInputTokens(), item.Position, token);
        }

        public bool IsTailNullable(DotItem item)
        {
            return tables.IsTailNullable(item.GetInputTokens(), item.Position);
        }

        private RuntimeProduction ToRt(Production production)
        {
            return new RuntimeProduction(
                production.Index,
                production.Outcome.Index,
                production.Input.Select(sym => sym.Index));
        }

        private static int[] BuildTokenComplexity(Grammar grammar)
        {
            var result =  grammar.Symbols.CreateCompatibleArray(IndexingConstants.NoIndex);

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
            return grammar.Symbols[token].Productions.SelectMany(rule => rule.InputTokens);
        }
    }
}
