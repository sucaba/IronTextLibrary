using IronText.Framework;

namespace IronText.Lib.IL
{
    [Demand]
    public interface WantFieldAttr 
        : WantFieldAttrThen<WantFieldAttr>
        , WantFieldType
    {
    }
}
