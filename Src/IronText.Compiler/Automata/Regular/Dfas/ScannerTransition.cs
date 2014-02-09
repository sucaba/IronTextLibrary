using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using IronText.Reflection.Reporting;

namespace IronText.Automata.Regular
{
    sealed class ScannerTransition : IScannerTransition
    {
        public ScannerTransition(IEnumerable<CharRange> ranges, IScannerState destination)
        {
            this.CharRanges = new ReadOnlyCollection<CharRange>(ranges.ToArray());
            this.Destination = destination;
        }

        public ReadOnlyCollection<CharRange> CharRanges { get; private set; }

        public IScannerState Destination { get; private set; }
    }
}
