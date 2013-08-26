using IronText.Framework;
using IronText.Lib.Shared;

namespace IronText.Lib.IL
{
    [Demand]
    public interface CustomAttributesSyntax<T>
    {
        [Parse(".custom")]
        T CustomAttribute(Ref<Types> customType);
    }
}
