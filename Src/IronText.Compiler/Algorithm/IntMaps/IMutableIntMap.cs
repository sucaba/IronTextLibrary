
namespace IronText.Algorithm
{
    public interface IMutableIntMap<TAttr>
        : IIntMap<TAttr>
    {
        new TAttr DefaultValue { get; set; }

        void Set(IntArrow<TAttr> newArrow);

        void Set(IntervalIntSet set, TAttr attr);

        void Clear(IntInterval interval);

        void Clear();
    }
}
