using System;

namespace IronText.Algorithm
{
    public class MutableTable<T>
        : IMutableTable<T>
        where T : IEquatable<T>
    {
        private readonly T[,] table;

        public MutableTable(T[,] table)
        {
            this.table = table;
        }

        public MutableTable(int rowCount, int columnCount)
        {
            this.table = new T[rowCount, columnCount];
        }

        public void Set(int row, int col, T value)
        {
            table[row, col] = value;
        }

        public int RowCount
        {
            get { return table.GetLength(0); }
        }

        public int ColumnCount
        {
            get { return table.GetLength(1); }
        }

        public T Get(int row, int column)
        {
            return table[row, column];
        }

        public IIntMap<T> GetRow(int row)
        {
            var result = new MutableIntMap<T>();

            int columnCount = ColumnCount;

            int first = 0;

            for (int c = 0; c != columnCount; ++c)
            {
                int nextC = c + 1;
                if (nextC != columnCount && table[row, nextC].Equals(table[row, c]))
                {
                    continue;
                }

                result.Set(new IntArrow<T>(first, c, table[row, c]));
                first = nextC;
            }

            return result;
        }
    }
}
