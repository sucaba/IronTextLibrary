using IronText.Framework;

namespace IronText.Lib.IL
{
    [Demand]
    public interface WantCallConvThen<TNext>
    {
        [ParseGet("instance")]
        TNext Instance { get; }

        [ParseGet("explicit")]
        TNext Explicit { get; }
    }
}
