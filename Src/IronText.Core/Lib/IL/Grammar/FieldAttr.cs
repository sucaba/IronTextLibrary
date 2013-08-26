using System;

namespace IronText.Lib.IL
{
    [Flags]
    public enum FieldAttr
    {
        NonPublic = 0x0,
        Public    = 0x1,
        Static    = 0x2,

        Default   = NonPublic
    }
}
