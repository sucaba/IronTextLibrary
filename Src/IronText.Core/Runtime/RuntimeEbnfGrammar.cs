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

        public RuntimeEbnfGrammar(EbnfGrammar grammar)
        {
            this.grammar = grammar;

            this.MaxRuleSize = grammar.Productions.Select(r => r.Pattern.Length).Max();
        }

        public ProductionCollection Productions { get { return grammar.Productions; } }

        public bool IsNullable(int token) { return grammar.IsNullable(token); }

        public IEnumerable<Production> GetProductions(int outcomeToken)
        {
            return grammar.Symbols[outcomeToken].Productions;
        }

        public IEnumerable<int> EnumerateTokens()
        {
            return grammar.Symbols.Select(ti => ti.Index);
        }

        public bool IsTerminal(int token)
        {
            return grammar.Symbols[token].IsTerminal;
        }

        public bool IsBeacon(int token)
        {
            return grammar.Symbols[token].Categories.Has(TokenCategory.Beacon);
        }

        public bool IsDontInsert(int token)
        {
            return grammar.Symbols[token].Categories.Has(TokenCategory.DoNotInsert);
        }

        public bool IsDontDelete(int token)
        {
            return grammar.Symbols[token].Categories.Has(TokenCategory.DoNotDelete);
        }

        public int SymbolCount 
        {
            get { return grammar.SymbolCount; }
        }

        public string SymbolName(int token)
        {
            return grammar.SymbolName(token);
        }

        public TokenCategory GetTokenCategories(int token)
        {
            return grammar.GetTokenCategories(token);
        }

        public int MaxRuleSize { get; private set; }
    }

}
