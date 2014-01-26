using System;
using System.Collections.Generic;
using System.Reflection;
using IronText.Framework;

namespace IronText.Extensibility
{
    public interface ICilMetadata
    {
        ICilMetadata Parent { get; }

        MemberInfo   Member { get; }


        bool Validate(ILogging logging);

        IEnumerable<ICilMetadata> GetChildren();

        void Bind(ICilMetadata parent, MemberInfo member);

        IEnumerable<CilSymbolFeature<Precedence>> GetSymbolPrecedence();

        IEnumerable<CilSymbolFeature<CilContextProvider>> GetSymbolContextProviders();

        IEnumerable<CilSymbolRef> GetSymbolsInCategory(SymbolCategory category);

        IEnumerable<CilProduction> GetProductions(IEnumerable<CilSymbolRef> outcomes);

        IEnumerable<CilMerger> GetMergers(IEnumerable<CilSymbolRef> symbols);

        IEnumerable<CilScanProduction> GetScanProductions();

        IEnumerable<ReportBuilder> GetReportBuilders();
    }
}
