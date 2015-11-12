using IronText.Diagnostics;
using System.Collections.ObjectModel;

namespace IronText.Reflection.Reporting
{
    public interface IScannerAutomata
    {
        ReadOnlyCollection<IScannerState> States { get; }

        void DescribeGraph(IGraphView view);
    }
}
