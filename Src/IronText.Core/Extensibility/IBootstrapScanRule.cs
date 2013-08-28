using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Extensibility
{
    internal interface IBootstrapScanRule
    {
        string BootstrapRegexPattern { get; }
    }
}
