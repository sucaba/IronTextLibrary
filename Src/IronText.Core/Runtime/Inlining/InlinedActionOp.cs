namespace IronText.Runtime.Inlining
{
    public enum InlinedActionOp : byte
    {
        Init      = 0,
        Read      = 1,
        Reduce    = 2,
        Shifted   = 3,
        RetDirect = 4,
        Ret       = 5,
    }
}
