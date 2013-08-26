using IronText.Framework;

namespace IronText.Lib.IL
{
    [Demand]
    public interface WantNameThen<TNext>
    {
        [Parse]
        TNext Named(string methodName);
    }
}
