using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Reflection.Indexing
{
    /// <summary>
    /// Provides 1:1 map between grammar entities and integer indexes
    /// </summary>
    public interface IGrammarIndexer
    {
        int        IndexOf(SymbolBase symbol);

        int        IndexOf(Production production);

        SymbolBase GetAnySymbol(int symbolIndex);

        Symbol     GetSymbol(int symbolIndex);

        Production GetProduction(int prodIndex);
    }
}
