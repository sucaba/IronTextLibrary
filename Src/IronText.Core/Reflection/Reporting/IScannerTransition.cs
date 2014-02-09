using System.Collections.ObjectModel;

namespace IronText.Reflection.Reporting
{
    public interface IScannerTransition
    {
        ReadOnlyCollection<CharRange> CharRanges { get; }

        IScannerState Destination { get; }
    }
}
