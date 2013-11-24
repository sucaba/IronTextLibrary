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
        }

        public ProductionCollection Productions { get { return grammar.Productions; } }

        public bool IsNullable(int token) { return grammar.IsNullable(token); }

        public IEnumerable<Production> GetProductions(int outcomeToken)
        {
            return grammar.GetProductions(outcomeToken);
        }

        public IEnumerable<int> EnumerateTokens()
        {
            return grammar.EnumerateTokens();
        }

        public bool IsTerminal(int token)
        {
            return grammar.IsTerminal(token);
        }

        public bool IsBeacon(int token)
        {
            return grammar.IsBeacon(token);
        }

        public bool IsDontInsert(int token)
        {
            return grammar.IsDontInsert(token);
        }

        public bool IsDontDelete(int token)
        {
            return grammar.IsDontDelete(token);
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

        public int MaxRuleSize { get { return grammar.MaxRuleSize; } }
    }

}
