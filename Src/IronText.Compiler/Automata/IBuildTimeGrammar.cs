using System.Collections.Generic;
using IronText.Reflection;

namespace IronText.Automata
{
    interface IBuildtimeGrammar
    {
        int SymbolCount { get; }

        BuildtimeProduction AugmentedProduction { get; }

        IEnumerable<BuildtimeProduction> Productions { get; }

        BuildtimeProduction GetProduction(int index);

        Precedence GetProductionPrecedence(int prodId);

        IEnumerable<BuildtimeProduction> GetProductions(int leftToken);

        bool IsTerminal(int token);

        Precedence GetTermPrecedence(int token);

        string GetTokenName(int token);
    }
}