using IronText.Framework;

namespace IronText.Lib.IL
{
    [Demand]
    public interface WantImplAttrThen<TNext>
    {
        [ParseGet("cil")]
        TNext Cil { get; }

        [ParseGet("forwardref")]
        TNext ForwardRef { get; }

        [ParseGet("internalcall")]
        TNext InternalCall { get; }

        [ParseGet("managed")]
        TNext Managed { get; }

        [ParseGet("native")]
        TNext Native { get; }

        [ParseGet("noinlining")]
        TNext NoInlining { get; }

        [ParseGet("nooptimization")]
        TNext NoOptimization { get; }

        [ParseGet("runtime")]
        TNext Runtime { get; }

        [ParseGet("synchronized")]
        TNext Synchronized { get; }

        [ParseGet("unmanaged")]
        TNext Unmanaged { get; }
    }
}
