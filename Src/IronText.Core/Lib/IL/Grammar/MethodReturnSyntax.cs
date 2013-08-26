using IronText.Framework;
using IronText.Lib.Shared;

namespace IronText.Lib.IL
{
    [Demand]
    public interface WantReturnTypeThen<TNext>
    {
        [Parse]
        TNext Returning(Ref<Types> resultType);
    }
}
