using System.Collections.ObjectModel;

namespace IronText.Reporting
{
    public interface IScannerAutomata
    {
        ReadOnlyCollection<IScannerState> States { get; }
    }
}
