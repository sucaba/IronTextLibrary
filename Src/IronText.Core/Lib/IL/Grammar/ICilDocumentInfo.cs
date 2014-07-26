using System;

namespace IronText.Lib.IL
{
    public interface ICilDocumentInfo
    {
        Guid               Mvid { get; }

        IResolutionScopeNs ResolutionScopeNs { get; }

        ITypeNs            Types { get; }

        IMethodNs          Methods { get; }
    }
}
