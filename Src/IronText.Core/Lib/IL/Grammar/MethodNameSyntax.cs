using IronText.Framework;

namespace IronText.Lib.IL
{
    [Demand]
    public interface WantNameThen<TNext>
    {
        [Produce]
        TNext Named(string methodName);
    }
}
