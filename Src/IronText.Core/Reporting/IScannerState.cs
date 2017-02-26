using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace IronText.Reporting
{
    public interface IScannerState
    {
        int Index { get; }

        bool IsAccepting { get; }

        bool IsFinal { get; }

        bool IsNewline { get; }

        IReadOnlyList<int> Actions { get; }

        IScannerState TunnelState { get; }

        ReadOnlyCollection<IScannerTransition> Transitions { get; }
    }
}
