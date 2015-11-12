using System.Collections.Generic;
using System.Linq;
using IronText.Reflection;
using System;

namespace IronText.Runtime
{
    internal class RuntimeGrammar
    {
        private readonly Grammar grammar;
        private readonly bool[]  isNullable;
        private readonly int     lastToken;
        private readonly RuntimeProduction[] _runtimeProductions;
        private readonly SymbolCategory[] _symbolCategories;
        private readonly string[] _symbolNames;

        public RuntimeGrammar(Grammar grammar)
        {
            this.grammar = grammar;
            IRuntimeNullableFirstTables tables = new NullableFirstTables(grammar);
            this.isNullable  = tables.TokenToNullable;
            this.MaxRuleSize = tables.MaxRuleSize;
            this.lastToken   = grammar.Symbols.LastIndex;
            this._runtimeProductions =
                Array.ConvertAll(
                    grammar.Productions.ToArray(),
                    p => new RuntimeProduction(
                        p.Index,
                        p.OutcomeToken,
                        p.InputTokens.ToArray()));
            this.StartProductionIndex = grammar.Productions.StartIndex;
            this.LastProductionIndex = grammar.Productions.LastIndex;

            this._symbolCategories = Array.ConvertAll(grammar.Symbols.ToArray(), s => s.Categories);
            this._symbolNames = Array.ConvertAll(grammar.Symbols.ToArray(), s => s.Name);
        }

        public int MaxRuleSize          { get; private set; }

        public int StartProductionIndex { get; private set; }

        public int LastProductionIndex  { get; private set; }

        public RuntimeProduction[] RuntimeProductions { get { return _runtimeProductions; } }

        public bool IsNullable(int token) { return isNullable[token]; }

        public IEnumerable<RuntimeProduction> GetNullableProductions(int outcome)
        {
            return from prod in _runtimeProductions
                   where prod.OutcomeToken == outcome && prod.Input.All(IsNullable)
                   orderby prod.InputLength ascending
                   select prod;
        }

        /// <summary>
        /// Enumerates all tokens except predefined
        /// </summary>
        /// <returns></returns>
        public IEnumerable<int> EnumerateTokens()
        {
            return from s in grammar.Symbols
                   where !s.IsPredefined
                   select s.Index;
        }

        public bool IsTerminal(int token)
        {
            return grammar.Symbols[token].IsTerminal;
        }

        public bool IsBeacon(int token)
        {
            return _symbolCategories[token].Has(SymbolCategory.Beacon);
        }

        public bool IsDontInsert(int token)
        {
            return _symbolCategories[token].Has(SymbolCategory.DoNotInsert);
        }

        public bool IsDontDelete(int token)
        {
            return _symbolCategories[token].Has(SymbolCategory.DoNotDelete);
        }

        public int LastToken 
        {
            get { return lastToken; }
        }

        public string SymbolName(int token)
        {
            return _symbolNames[token];
        }

        public SymbolCategory GetTokenCategories(int token)
        {
            return this._symbolCategories[token];
        }
    }
}
