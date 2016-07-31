using IronText.Algorithm;
using IronText.Automata.Lalr1;
using IronText.Collections;
using IronText.Runtime;
using System.Collections.Generic;
using System.Linq;

namespace IronText.MetadataCompiler
{
    class ParserBytecodeProvider
    {
        private const int SharedFailurePos = 0;

        public ParserBytecodeProvider(CanonicalLrDfaTable parserTable)
        {
            var instructions = new List<ParserInstruction>();

            CompileSharedFailureAction(instructions);

            var table = parserTable.ParserActionTable;
            int rowCount = table.RowCount;
            int columnCount = table.ColumnCount;

            var startTable = new MutableTable<int>(rowCount, columnCount);

            for (int r = 0; r != rowCount; ++r)
                for (int c = 0; c != columnCount; ++c)
                {
                    var decision = table.Get(r, c);
                    if (decision == ParserDecision.NoAlternatives)
                    {
                        startTable.Set(r, c, SharedFailurePos);
                        continue;
                    }

                    startTable.Set(r, c, instructions.Count);

                    CompileAmbiguous(instructions, decision);
                }

            this.Instructions = instructions.ToArray();
            this.StartTable = startTable;
        }

        private static void CompileSharedFailureAction(List<ParserInstruction> instructions)
        {
            instructions.Add(ParserInstruction.FailAction);
            CompileBranchEnd(instructions);
        }

        private static void CompileAmbiguous(List<ParserInstruction> instructions, ParserDecision decision)
        {
            int forkPos = instructions.Count;

            foreach (var alternative in decision.Alternatives().Skip(1))
            {
                instructions.Add(ForkStub);
            }

            bool first = true;
            foreach (var alternative in decision.Alternatives())
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    instructions[forkPos++] = ParserInstruction.Fork(instructions.Count);
                }

                Compile(instructions, alternative);
            }
        }

        private static ParserInstruction ForkStub => ParserInstruction.InternalErrorAction;

        private static void Compile(List<ParserInstruction> instructions, ParserDecision decision)
        {
            foreach (var instruction in decision.Instructions)
            {
                instructions.Add(instruction);
            }

            CompileBranchEnd(instructions);
        }

        private static void CompileBranchEnd(List<ParserInstruction> instructions)
        {
            switch (instructions.Last().Operation)
            {
                case ParserOperation.Resolve:
                case ParserOperation.Reduce:
                    instructions.Add(ParserInstruction.RestartAction);
                    break;
                case ParserOperation.Shift:
                    instructions.Add(ParserInstruction.ExitAction);
                    break;
                default:
                    // safety instruction to avoid invalid instruction access
                    instructions.Add(ParserInstruction.InternalErrorAction);
                    break;
            }
        }

        public ParserInstruction[] Instructions { get; }

        public ITable<int>         StartTable   { get; }
    }
}
