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
        private readonly List<ParserInstruction> instructions;

        public ParserBytecodeProvider(CanonicalLrDfaTable parserTable)
        {
            this.instructions = new List<ParserInstruction>();

            CompileSharedFailureAction();

            var table = parserTable.DecisionTable;
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
                    }
                    else
                    {
                        startTable.Set(r, c, instructions.Count);
                        CompileAmbiguousDecision(instructions, decision);
                    }
                }

            this.Instructions = instructions.ToArray();
            this.StartTable = startTable;
        }

        public ParserInstruction[] Instructions { get; }

        public ITable<int>         StartTable   { get; }

        private void CompileSharedFailureAction()
        {
            instructions.Add(ParserInstruction.FailAction);
            CompileBranchEnd();
        }

        private void CompileAmbiguousDecision(List<ParserInstruction> instructions, ParserDecision decision)
        {
            int forkPos = instructions.Count;

            foreach (var alternative in decision.OtherAlternatives())
            {
                instructions.Add(ForkStub);
            }

            CompileDecision(decision);

            foreach (var alternative in decision.OtherAlternatives())
            {
                instructions[forkPos++] = ParserInstruction.Fork(instructions.Count);
                CompileDecision(alternative);
            }
        }

        private static ParserInstruction ForkStub => ParserInstruction.InternalErrorAction;

        private void CompileDecision(ParserDecision decision)
        {
            instructions.AddRange(decision.Instructions);

            CompileBranchEnd();
        }

        private void CompileBranchEnd()
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
    }
}
