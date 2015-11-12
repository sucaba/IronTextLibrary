using System.Collections.Generic;
using System.Linq;
using IronText.Reflection;
using System;

namespace IronText.Runtime
{
    internal class RuntimeGrammar
    {
        private readonly bool[]              tokenIsNullable;
        private readonly bool[]              tokenIsTerminal;
        private readonly SymbolCategory[]    tokenCategories;
        private readonly string[]            tokenNames;
        private readonly int[]               nonPredefinedTokens;

        public RuntimeGrammar(
            int                 tokenCount,
            RuntimeProduction[] runtimeProductions,
            bool[]              tokenIsNullable,
            bool[]              tokenIsTerminal,
            int[]               nonPredefinedTokens,
            SymbolCategory[]    tokenCategories,
            string[]            tokenNames)
        {
            this.TokenCount           = tokenCount;
            this.tokenIsNullable      = tokenIsNullable;
            this.tokenIsTerminal      = tokenIsTerminal;
            this.tokenCategories      = tokenCategories;
            this.tokenNames           = tokenNames;
            this.nonPredefinedTokens  = nonPredefinedTokens;

            this.RuntimeProductions   = runtimeProductions;
            this.MaxProductionLength  = runtimeProductions.Select(r => r.InputLength).Max();
            this.LastProductionIndex  = runtimeProductions.Length;
        }

        public int MaxProductionLength  { get; private set; }

        public int StartProductionIndex { get { return 0; } }

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

        public int TokenCount { get; private set; }

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
