using System.Collections.Generic;
using System.Linq;
using IronText.Reflection;

namespace IronText.Runtime
{
    internal class RuntimeGrammar
    {
        private readonly Grammar grammar;
        private readonly bool[]  isNullable;
        private readonly int     lastToken;

        public RuntimeGrammar(Grammar grammar)
        {
            this.grammar = grammar;
            IRuntimeNullableFirstTables tables = new NullableFirstTables(grammar);
            this.isNullable  = tables.TokenToNullable;
            this.MaxRuleSize = tables.MaxRuleSize;
            this.lastToken   = grammar.Symbols.LastIndex;
        }

        public int MaxRuleSize { get; private set; }

        public int StartProductionIndex { get {  return grammar.Productions.StartIndex; } }

        public int LastProductionIndex  { get {  return grammar.Productions.LastIndex; } }

        public ProductionCollection Productions { get { return grammar.Productions; } }

        public bool IsNullable(int token) { return isNullable[token]; }

        public IEnumerable<Production> GetNullableProductions(int outcome)
        {
            return 
               from r in GetProductions(outcome)
               where r.InputTokens.All(IsNullable)
               orderby r.InputTokens.Length ascending
               select r;
        }

        private IEnumerable<Production> GetProductions(int outcome)
        {
            return grammar.Symbols[outcome].Productions;
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
            return grammar.Symbols[token].Categories.Has(SymbolCategory.Beacon);
        }

        public bool IsDontInsert(int token)
        {
            return grammar.Symbols[token].Categories.Has(SymbolCategory.DoNotInsert);
        }

        public bool IsDontDelete(int token)
        {
            return grammar.Symbols[token].Categories.Has(SymbolCategory.DoNotDelete);
        }

        public int LastToken 
        {
            get { return lastToken; }
        }

        public string SymbolName(int token)
        {
            return grammar.Symbols[token].Name;
        }

        public SymbolCategory GetTokenCategories(int token)
        {
            return grammar.Symbols[token].Categories;
        }
    }

}
