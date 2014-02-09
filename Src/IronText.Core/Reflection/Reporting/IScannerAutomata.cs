using System.Collections.ObjectModel;
using IronText.Diagnostics;

namespace IronText.Reflection.Reporting
{
    public interface IScannerAutomata
    {
        ReadOnlyCollection<IScannerState> States { get; }

        void DescribeGraph(IGraphView view);
    }
}
