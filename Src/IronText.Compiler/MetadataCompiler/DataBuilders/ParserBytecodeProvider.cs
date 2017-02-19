using IronText.Algorithm;
using IronText.Automata;
using IronText.Automata.Lalr1;
using IronText.Collections;
using IronText.Runtime;
using System.Collections.Generic;
using System.Linq;

namespace IronText.MetadataCompiler
{
    class ParserBytecodeProvider : IParserBytecodeProvider
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
                        startTable.Set(r, c, NextInstructionPos);
                        CompileAmbiguousDecision(decision);
                    }
                }

            this.Instructions = instructions.ToArray();
            instructions.Clear();
            this.StartTable = startTable;
        }

        public ParserInstruction[] Instructions { get; }

        public ITable<int>         StartTable   { get; }

        private int NextInstructionPos => instructions.Count;

        private static ParserInstruction ForkStub => ParserInstruction.InternalErrorAction;

        private void CompileSharedFailureAction()
        {
            instructions.Add(ParserInstruction.FailAction);
            CompileBranchEnd();
        }

        private void CompileAmbiguousDecision(ParserDecision decision)
        {
            int forkInstructionPos = NextInstructionPos;

            foreach (var other in decision.OtherAlternatives())
            {
                instructions.Add(ForkStub);
            }

            CompileDecision(decision);

            foreach (var other in decision.OtherAlternatives())
            {
                instructions[forkInstructionPos++] = ParserInstruction.Fork(NextInstructionPos);
                CompileDecision(other);
            }
        }

        private void CompileDecision(ParserDecision decision)
        {
            instructions.Add(decision.Instruction);

            CompileBranchEnd();
        }

        private void CompileBranchEnd()
        {
            switch (instructions.Last().Operation)
            {
                case ParserOperation.Reduce:
                    instructions.Add(ParserInstruction.Return(instructions.Last().Production));
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
