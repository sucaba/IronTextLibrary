using IronText.Framework;
using IronText.Lib.Shared;

namespace IronText.Lib.IL
{
    [Demand]
    public interface WantFieldType
    {
        [Produce]
        WantFieldName OfType(Ref<Types> type);
    }
}
