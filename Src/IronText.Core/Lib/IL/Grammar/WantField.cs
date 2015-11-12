using IronText.Framework;

namespace IronText.Lib.IL
{
    [Demand]
    public interface WantField : WantFieldAttr
    {
        [Produce("[", null, "]")]
        WantFieldAttr Repeat(int length);
    }
}
