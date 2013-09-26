#if false
using System.Collections.Generic;
using IronText.Algorithm;
using IronText.Framework;
using IronText.Lib.Shared;

namespace IronText.Lib.IL.Generators
{
    public class SparseReadOnlyTableGenerator
    {
        private readonly SparseTable<int> table;
        private readonly Pipe<EmitSyntax> LdRow;
        private readonly Pipe<EmitSyntax> LdCol;

        public SparseReadOnlyTableGenerator(SparseTable<int> newTable, Pipe<EmitSyntax> ldRow, Pipe<EmitSyntax> ldCol)
        {
            this.table = newTable;
            this.LdRow = ldRow;
            this.LdCol = ldCol;
        }

        public EmitSyntax Build(EmitSyntax emit)
        {
            int rows = table.RowCount;
            int columns = table.ColumnCount;

            // Dispatch info index
            Ref<Labels> DISP_INFO = emit.Labels.Generate().GetRef();
            // Return no-data result
            Ref<Labels> RET_NONE = emit.Labels.Generate().GetRef();
            // Exit
            Ref<Labels> END = emit.Labels.Generate().GetRef();
            Ref<Labels>[] rowLabels = new Ref<Labels>[rows];
            for (int i = 0; i != rows; ++i)
            {
                rowLabels[i] = emit.Labels.Generate().GetRef();
            }

            emit
                .Do(LdRow)
                .Switch(rowLabels)
                .Br(RET_NONE)
                ;

            for (int i = 0; i != rows; ++i)
            {
                emit
                    .Label(rowLabels[i].Def)
                    .Ldc_I4(table.RowToOffset[i])
                    .Br(DISP_INFO);
            }

            int infoCount = table.Info.Count;
            var infoLabels = new Ref<Labels>[infoCount];
            for (int i = 0; i != infoCount; ++i)
            {
                infoLabels[i] = emit.Labels.Generate().GetRef();
            }

            emit
                .Label(DISP_INFO.Def)
                // Row offset should already be in stack
                .Do(LdCol)
                .Add()
                .Switch(infoLabels)
                .Br(RET_NONE);
            for (int i = 0; i != infoCount; ++i)
            {
                emit
                    .Label(infoLabels[i].Def)
                    .Do(LdRow)
                    .Ldc_I4(table.Check[i])
                    .Bne_Un(RET_NONE)
                    .Ldc_I4(table.Info[i])
                    .Br(END)
                    ;
            }

            emit
                .Label(RET_NONE.Def)
                .Ldc_I4(0)
                ;
            return emit.Label(END.Def);
        }
    }
}
#endif
