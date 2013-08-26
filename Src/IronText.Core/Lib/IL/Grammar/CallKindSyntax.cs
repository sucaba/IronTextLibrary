using IronText.Framework;

namespace IronText.Lib.IL
{
    [Demand]
    public interface WantCallKindThen<TNext>
    {
        [ParseGet("default")]
        TNext Default { get; }

        [ParseGet("vararg")]
        TNext VarArg { get; }

        [ParseGet("unmanaged", "cdecl")]
        TNext Cdecl { get; }

        [ParseGet("unmanaged", "stdcall")]
        TNext StdCall { get; }

        [ParseGet("unmanaged", "thiscall")]
        TNext ThisCall { get; }

        [ParseGet("unmanaged", "fastcall")]
        TNext FastCall { get; }
    }
}
