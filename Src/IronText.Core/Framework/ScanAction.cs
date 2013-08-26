using System;

namespace IronText.Framework
{
    [Flags]
    public enum ScanAction
    {
        Emit    = 0x0,
        Push    = 0x1,
        Pop     = 0x2,
        Mode    = 0x3,
    }
}
