using IronText.Framework;

namespace IronText.Lib.IL
{
    [Demand]
    public interface PInvAttrSyntax
    {
        [Parse(")")]
        WantMethAttr EndPinvokeimpl();

        [ParseGet("nomangle")]
        PInvAttrSyntax Nomangle { get; }

        [ParseGet("ansi")]
        PInvAttrSyntax Ansi { get; }

        [ParseGet("unicode")]
        PInvAttrSyntax Unicode { get; }

        [ParseGet("autochar")]
        PInvAttrSyntax Autochar { get; }

        [ParseGet("lasterr")]
        PInvAttrSyntax Lasterr { get; }

        [ParseGet("winapi")]
        PInvAttrSyntax Winapi { get; }

        [ParseGet("cdecl")]
        PInvAttrSyntax Cdecl { get; }

        [ParseGet("stdcall")]
        PInvAttrSyntax Stdcall { get; }

        [ParseGet("thiscall")]
        PInvAttrSyntax Thiscall { get; }

        [ParseGet("fastcall")]
        PInvAttrSyntax Fastcall { get; }
    }
}
