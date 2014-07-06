using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Misc
{
    public interface IHasIdentity
    {
        object Identity { get; }
    }
}
