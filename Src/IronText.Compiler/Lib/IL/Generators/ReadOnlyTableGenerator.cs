using System.Collections.Generic;
using IronText.Algorithm;
using IronText.Framework;
using IronText.Lib.Shared;

namespace IronText.Lib.IL.Generators
{
    public class ReadOnlyTableGenerator
    {
        private readonly ITable<int>      table;
        private readonly Pipe<EmitSyntax> LdRow;
        private readonly Pipe<EmitSyntax> LdCol;

        public ReadOnlyTableGenerator(ITable<int> newTable, Pipe<EmitSyntax> ldRow, Pipe<EmitSyntax> ldCol)
        {
            this.table = newTable;
            this.LdRow = ldRow;
            this.LdCol = ldCol;
        }

        public EmitSyntax Build(EmitSyntax emit)
        {
            int rows = table.RowCount;
            int columns = table.ColumnCount;

            Ref<Labels> END = emit.Labels.Generate().GetRef();
            Ref<Labels>[] labels = new Ref<Labels>[rows];
            for (int i = 0; i != rows; ++i)
            {
                labels[i] = emit.Labels.Generate().GetRef();
            }

            emit
                .Do(LdRow)
                .Switch(labels)
                .Ldc_I4(-1)
                .Br(END)
                ;

            for (int i = 0; i != rows; ++i)
            {
                var intervalToValue = new List<IntArrow<int>>();

                // TODO: Optimize. Following causes huge size of GetParserAction method.
                for (int c = 0; c != columns; ++c)
                {
                    int value = table.Get(i, c);
                    intervalToValue.Add(new IntArrow<int>(c, value));
                }

                var switchEmitter = SwitchGenerator.Sparse(table.GetRow(i), new IntInterval(0, columns));

                emit.Label(labels[i].Def);
                switchEmitter.Build(emit, LdCol, SwitchGenerator.LdValueAction(END));
            }

            return emit .Label(END.Def);
        }
    }
}
