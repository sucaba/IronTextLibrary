using System;
using IronText.Algorithm;
using IronText.Lib.IL;
using IronText.Lib.IL.Generators;
using NUnit.Framework;

namespace IronText.Tests.Lib.IL.Generators
{
    [TestFixture]
    public class ReadOnlyTableGeneratorTest
    {
        [Test]
        public void Test()
        {
            const int rows = 2;
            const int columns = 3;

            int[,] table = new int[rows, columns];
            for (int r = 0; r != rows; ++r)
            for (int c = 0; c != columns; ++c)
            {
                table[r,c] = r * columns + c;
            }

            var target = new ReadOnlyTableGenerator(new MutableTable<int>(table), emit => emit.Ldarg(0), emit=> emit.Ldarg(1));

            var getCell = new CachedMethod<Func<int,int,int>>("TableSerializerTest.Assembly0", (emit, args) => { target.Build(emit); return emit.Ret(); }).Delegate;
            for (int r = 0; r != rows; ++r)
                for (int c = 0; c != columns; ++c)
                {
                    Assert.AreEqual(table[r, c], getCell(r, c), "r={0}, c={1}", r, c);
                }
        }
    }
}
