using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework.Reflection;

namespace IronText.Framework
{
    internal class RuntimeEbnfGrammar
    {
        private readonly EbnfGrammar grammar;
        private readonly bool[]      isNullable;
        private readonly int         symbolCount;

        public RuntimeEbnfGrammar(EbnfGrammar grammar)
        {
            this.grammar = grammar;
            IRuntimeNullableFirstTables tables = new NullableFirstTables(grammar);
            this.isNullable  = tables.TokenToNullable;
            this.MaxRuleSize = tables.MaxRuleSize;
            this.symbolCount = grammar.Symbols.Count;
        }

        public int MaxRuleSize { get; private set; }

        public ProductionCollection Productions { get { return grammar.Productions; } }

        public bool IsNullable(int token) { return isNullable[token]; }

        public IEnumerable<Production> GetProductions(int outcome)
        {
            return grammar.Symbols[outcome].Productions;
        }

        public IEnumerable<int> EnumerateTokens()
        {
            return from s in grammar.Symbols
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

        public int SymbolCount 
        {
            get { return symbolCount; }
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
