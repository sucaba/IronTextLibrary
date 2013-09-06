
namespace IronText.Algorithm
{
    public interface ITable<T>
    {
        int RowCount { get; }

        int ColumnCount { get; }

        T Get(int row, int column);

        IIntMap<T> GetRow(int row);
    }
}
