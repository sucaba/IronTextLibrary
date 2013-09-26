using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace IronText.Algorithm
{
    public class SparseTable<T> : ITable<T> where T : IEquatable<T>
    {
        // index -> row
        private List<int> check;

        // index -> element
        private List<T> info;


        // row -> row start index
        private int[] row2offset;
        private readonly int columnCount;

        public SparseTable(ITable<T> table)
        {
            int rowCount    = table.RowCount;
            this.columnCount = table.ColumnCount;

            var info  = new List<T>(2 * columnCount);
            var check = new List<int>();
            this.row2offset  = new int[rowCount];

            for (int r = 0; r != rowCount; ++r)
            {
                int start = GetFitIndex(info, table, r);
                row2offset[r] = start;

                int growCount = (start + columnCount) - info.Count;
                if (growCount > 0)
                {
                    info.AddRange(Enumerable.Repeat(default(T), growCount));
                    check.AddRange(Enumerable.Repeat(-1, growCount));
                }
                
                for (int i = 0; i != columnCount; ++i)
                {
                    var value = table.Get(r, i);
                    if (!IsEmpty(value))
                    {
                        info[start + i] = value;
                        check[start + i] = r;
                    }
                }
            }

            this.info = info;
            this.check = check;
#if DIAGNOSTICS
            double gain = (4 * rowCount * columnCount - 4 * 2 * info.Count - 4 * row2offset.Length)
                     / (double)(4 * rowCount * columnCount);
            Debug.WriteLine(
                "table[{0},{1}] converted to INFO[{2}], CHECK[{2}] and ROW2OFFSET[{3}]: Gain={4}%",
                rowCount,
                columnCount,
                info.Count,
                row2offset.Length,
                gain * 100.0);
#endif
        }

        public ReadOnlyCollection<T>   Info { get { return new ReadOnlyCollection<T>(info); } }

        public ReadOnlyCollection<int> Check { get { return new ReadOnlyCollection<int>(check); } }

        public ReadOnlyCollection<int> RowToOffset { get { return new ReadOnlyCollection<int>(row2offset); } }

        private int GetFitIndex(List<T> info, ITable<T> table, int r)
        {
            int columnCount = table.ColumnCount;

            int result;
            for (result = 0; result != info.Count; ++result)
            {
                bool success = true;

                for (int c = 0; c != columnCount; ++c)
                {
                    if ((result + c) >= info.Count)
                    {
                        continue;
                    }

                    if (IsEmpty(info[result + c]) || IsEmpty(table.Get(r, c)))
                    {
                        continue;
                    }

                    success = false;
                    break;
                }

                if (success)
                {
                    break;
                }
            }

            return result;
        }

        public static bool IsEmpty(T value)
        {
            return EqualityComparer<T>.Default.Equals(value, default(T));
        }

        public int RowCount { get { return row2offset.Length; } }

        public int ColumnCount { get { return columnCount; } }

        public T Get(int row, int column)
        {
            int i = row2offset[row] + column;
            if (check[i] != row)
            {
                return default(T);
            }

            return info[i];
        }

        public IIntMap<T> GetRow(int row)
        {
            var result = new MutableIntMap<T>();

            int first = 0;

            for (int c = 0; c != columnCount; ++c)
            {
                int nextC = c + 1;
                if (nextC != columnCount && Get(row, nextC).Equals(Get(row, c)))
                {
                    continue;
                }

                result.Set(new IntArrow<T>(first, c, Get(row, c)));
                first = nextC;
            }

            return result;
        }
    }
}
