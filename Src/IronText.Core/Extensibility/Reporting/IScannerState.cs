using System;
using System.Collections.ObjectModel;

namespace IronText.Extensibility
{
    public interface IScannerState
    {
        int Index { get; }

        bool IsAccepting { get; }

        bool IsFinal { get; }

        bool IsNewline { get; }

        IScannerState TunnelState { get; }

        ReadOnlyCollection<IScannerTransition> Transitions { get; }
    }
}
