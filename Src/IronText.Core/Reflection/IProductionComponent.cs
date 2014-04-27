using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Reflection
{
    public interface IProductionComponent
    {
        int  Size { get; }

        void CopyTo(Symbol[] output, int startIndex);

        IProductionComponent[] Components { get; }

        T Accept<T>(IProductionComponentVisitor<T> visitor);
    }

    public interface IProductionComponentVisitor<T>
    {
        T VisitSymbol(Symbol symbol);

        T VisitProduction(Production production);
    }
}
