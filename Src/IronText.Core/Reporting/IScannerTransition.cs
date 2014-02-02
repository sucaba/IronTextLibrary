using System.Collections.ObjectModel;

namespace IronText.Reporting
{
    public interface IScannerTransition
    {
        ReadOnlyCollection<CharRange> CharRanges { get; }

        IScannerState Destination { get; }
    }
}
