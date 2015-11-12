using IronText.Framework;

namespace IronText.Lib.IL
{
    [Demand]
    public interface WantFieldName
    {
        [Produce]
        WantFieldAt Named(string fieldName);
    }
}
