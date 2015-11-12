using System.Collections.Generic;
using System.Linq;
using IronText.Reflection;
using System;

namespace IronText.Runtime
{
    internal class RuntimeGrammar
    {
        private readonly int                 lastToken;
        private readonly bool[]              tokenIsNullable;
        private readonly bool[]              tokenIsTerminal;
        private readonly SymbolCategory[]    tokenCategories;
        private readonly string[]            tokenNames;
        private readonly int[]               nonPredefinedTokens;

        public RuntimeGrammar(Grammar grammar)
        {
            IRuntimeNullableFirstTables tables = new NullableFirstTables(grammar);
            this.tokenIsNullable  = tables.TokenToNullable;
            this.tokenIsTerminal  = grammar.Symbols.CreateCompatibleArray(false);
            for (int tok = 0; tok != tokenIsTerminal.Length; ++tok)
            {
                tokenIsTerminal[tok] = grammar.Symbols[tok].IsTerminal;
            }

            this.MaxProductionLength = tables.MaxProductionLength;
            this.lastToken   = grammar.Symbols.LastIndex;
            this.RuntimeProductions =
                Array.ConvertAll(
                    grammar.Productions.ToArray(),
                    p => new RuntimeProduction(
                        p.Index,
                        p.OutcomeToken,
                        p.InputTokens.ToArray()));
            this.nonPredefinedTokens = (from s in grammar.Symbols
                                          where !s.IsPredefined
                                          select s.Index).ToArray();
            this.StartProductionIndex = grammar.Productions.StartIndex;
            this.LastProductionIndex = grammar.Productions.LastIndex;

            this.tokenCategories = Array.ConvertAll(grammar.Symbols.ToArray(), s => s.Categories);
            this.tokenNames = Array.ConvertAll(grammar.Symbols.ToArray(), s => s.Name);
        }

        public int MaxProductionLength  { get; private set; }

        public int StartProductionIndex { get; private set; }

        public int LastProductionIndex  { get; private set; }

        public RuntimeProduction[] RuntimeProductions { get; private set; }

        public bool IsNullable(int token) { return tokenIsNullable[token]; }

        public IEnumerable<RuntimeProduction> GetNullableProductions(int outcome)
        {
            return from prod in RuntimeProductions
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
            return nonPredefinedTokens;
        }

        public bool IsTerminal(int token)
        {
            return tokenIsTerminal[token];
        }

        public bool IsBeacon(int token)
        {
            return tokenCategories[token].Has(SymbolCategory.Beacon);
        }

        public bool IsDontInsert(int token)
        {
            return tokenCategories[token].Has(SymbolCategory.DoNotInsert);
        }

        public bool IsDontDelete(int token)
        {
            return tokenCategories[token].Has(SymbolCategory.DoNotDelete);
        }

        public int LastToken 
        {
            get { return lastToken; }
        }

        public string SymbolName(int token)
        {
            return tokenNames[token];
        }

        public SymbolCategory GetTokenCategories(int token)
        {
            return this.tokenCategories[token];
        }
    }
}
