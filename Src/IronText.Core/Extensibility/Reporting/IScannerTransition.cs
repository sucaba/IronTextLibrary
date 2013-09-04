using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace IronText.Extensibility
{
    public interface IScannerTransition
    {
        ReadOnlyCollection<CharRange> CharRanges { get; }

        IScannerState Destination { get; }
    }
}
