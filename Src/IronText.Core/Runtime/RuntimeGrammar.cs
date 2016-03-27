using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Runtime.Semantics;

namespace IronText.Runtime
{
    [Serializable]
    public class RuntimeGrammar
    {
        private readonly bool[]              tokenIsNullable;
        private readonly bool[]              tokenIsTerminal;
        private readonly SymbolCategory[]    tokenCategories;
        private readonly string[]            tokenNames;
        private readonly int[]               nonPredefinedTokens;
        private readonly RuntimeFormula[][]  stateToFormulas;
        private readonly RuntimeFormula[][]  productionToFormulas;

        public RuntimeGrammar(
            string[]            tokenNames,
            SymbolCategory[]    tokenCategories,
            bool[]              tokenIsNullable,
            bool[]              tokenIsTerminal,
            RuntimeProduction[] productions,
            RuntimeFormula[][]  stateToFormulas,
            RuntimeFormula[][]  productionToFormulas)
        {
            this.TokenCount           = tokenNames.Length;
            this.tokenIsNullable      = tokenIsNullable;
            this.tokenIsTerminal      = tokenIsTerminal;
            this.tokenCategories      = tokenCategories;
            this.tokenNames           = tokenNames;
            this.nonPredefinedTokens  = Enumerable.Range(0, TokenCount).Except(PredefinedTokens.All).ToArray();

            this.Productions          = productions;
            this.stateToFormulas      = stateToFormulas;
            this.productionToFormulas = productionToFormulas;
            this.MaxProductionLength  = productions.Select(r => r.InputLength).Max();
        }

        public int TokenCount { get; private set; }

        public int MaxProductionLength  { get; private set; }

        public RuntimeProduction[] Productions { get; private set; }

        public bool IsNullable(int token) { return tokenIsNullable[token]; }

        public IEnumerable<RuntimeProduction> GetNullableProductions(int outcome)
        {
            return from prod in Productions
                   where prod.Outcome == outcome && prod.Input.All(IsNullable)
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

        public string SymbolName(int token)
        {
            return tokenNames[token];
        }

        public SymbolCategory GetTokenCategories(int token)
        {
            return this.tokenCategories[token];
        }

        public RuntimeFormula[] GetShiftedFormulas(int shiftedState)
        {
            return this.stateToFormulas[shiftedState];
        }

        public RuntimeFormula[] GetReduceFormulas(int shiftedState)
        {
            return this.productionToFormulas[shiftedState];
        }
    }
}
