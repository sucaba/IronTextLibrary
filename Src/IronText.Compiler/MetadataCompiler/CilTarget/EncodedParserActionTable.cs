using IronText.Algorithm;
using IronText.Runtime;
using System.Collections.Generic;

namespace IronText.MetadataCompiler
{
    class EncodedParserActionTable : ITable<int>
    {
        private readonly ITable<ParserAction> original;

        public EncodedParserActionTable(ITable<ParserAction> original)
        {
            this.original = original;
        }

        public int ColumnCount => original.ColumnCount;

        public int RowCount => original.RowCount;

        public int Get(int row, int column) => ParserAction.Encode(original.Get(row, column));

        public IIntMap<int> GetRow(int row)
        {
            return new EncodedRow(original.GetRow(row));
        }

        class EncodedRow : IIntMap<int>
        {
            private IIntMap<ParserAction> original;

            public EncodedRow(IIntMap<ParserAction> original)
            {
                this.original = original;
            }

            public IntInterval Bounds => original.Bounds;


            public int DefaultValue => ParserAction.Encode(original.DefaultValue);

            public IEnumerable<IntArrow<int>> Enumerate()
            {
                foreach (var item in original.Enumerate())
                {
                    yield return new IntArrow<int>(item.Key, ParserAction.Encode(item.Value));
                }
            }

            public IEnumerable<IntArrow<int>> EnumerateCoverage(IntInterval bounds)
            {
                foreach (var item in original.EnumerateCoverage(bounds))
                {
                    yield return new IntArrow<int>(item.Key, ParserAction.Encode(item.Value));
                }
            }

            public int Get(int value)
            {
                return ParserAction.Encode(original.Get(value));
            }
        }
    }
}
