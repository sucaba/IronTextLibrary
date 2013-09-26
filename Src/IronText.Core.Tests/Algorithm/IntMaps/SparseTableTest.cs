using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Algorithm;
using NUnit.Framework;

namespace IronText.Tests.Algorithm
{
    [TestFixture]
    public class SparseTableTest
    {
        [Test]
        public void Test()
        {
            var data = new int[,]
            {
                { 2, 3, 0, 1, 0 },
                { 0, 3, 0, 0, 4 },
                { 2, 0, 5, 0, 0 },
                { 0, 0, 0, 5, 0 },
                { 2, 0, 0, 0, 4 },
            };

            var table = new MutableTable<int>(data);

            var sparseTable = new SparseTable<int>(table);

            for (int r = 0; r != table.RowCount; ++r)
                for (int c = 0; c != table.ColumnCount; ++c)
                {
                    Assert.AreEqual(table.Get(r, c), sparseTable.Get(r, c), "row=" + r +", col=" + c);
                }
        }
    }
}
