
namespace IronText.Algorithm
{
    public interface IMutableTable<T>
        : ITable<T>
    {
        void Set(int row, int col, T value);
    }
}
