using System;

namespace IronText.Lib.IL
{
    [Flags]
    public enum ParamAttr
    {
        None = 0x0,
        In   = 0x1,
        Out  = 0x2,
    }
}
