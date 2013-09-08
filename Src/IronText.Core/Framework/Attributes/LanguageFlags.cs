using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Framework
{
    public enum LanguageFlags
    {
        Default               = 0x0,

        ForceDeterministic    = 0x00,
        AllowNonDeterministic = 0x01,
        ForceNonDeterministic = 0x02,
    }
}
