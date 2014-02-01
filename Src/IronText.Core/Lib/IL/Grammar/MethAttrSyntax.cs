using IronText.Framework;
using IronText.Lib.Ctem;

namespace IronText.Lib.IL
{
    [Demand]
    public interface WantMethAttrThen<TNext>
    {
        [ParseGet("static")]
        TNext Static { get; }

        [ParseGet("public")]
        TNext Public { get; }

        [ParseGet("private")]
        TNext Private { get; }

        [ParseGet("family")]
        TNext Family { get; }

        [ParseGet("final")]
        TNext Final { get; }

        [ParseGet("specialname")]
        TNext Specialname { get; }

        [ParseGet("virtual")]
        TNext Virtual { get; }

        [ParseGet("abstract")]
        TNext Abstract { get; }

        [ParseGet("assembly")]
        TNext Assembly { get; }

        [ParseGet("famandassem")]
        TNext Famandassem { get; }

        [ParseGet("famorassem")]
        TNext Famorassem { get; }

        [ParseGet("privatescope")]
        TNext Privatescope { get; }

        [ParseGet("hidebysig")]
        TNext Hidebysig { get; }

        [ParseGet("newslot")]
        TNext Newslot { get; }

        [ParseGet("rtspecialname")]
        TNext Rtspecialname { get; }

        [ParseGet("unmanagedexp")]
        TNext Unmanagedexp { get; }

        [ParseGet("reqsecobj")]
        TNext Reqsecobj { get; }

        [Produce("pinvokeimpl", "(", null, "as", null)]
        PInvAttrSyntax BeginPinvokeimpl(QStr s1, QStr s2);

        [Produce("pinvokeimpl", "(", null)]
        PInvAttrSyntax BeginPinvokeimpl(QStr s);

        [Produce("pinvokeimpl", "(")]
        PInvAttrSyntax BeginPinvokeimpl();
    }
}
