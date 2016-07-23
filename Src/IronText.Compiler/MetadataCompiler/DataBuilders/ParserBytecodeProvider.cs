using IronText.Algorithm;
using IronText.Automata.Lalr1;
using IronText.Runtime;
using System.Collections.Generic;
using System.Linq;

namespace IronText.MetadataCompiler
{
    class ParserBytecodeProvider
    {
        public ParserBytecodeProvider(ILrParserTable parserTable)
        {
            var instructions = new List<ParserInstruction>();

            var table       = parserTable.ParserActionTable;
            int rowCount    = table.RowCount;
            int columnCount = table.ColumnCount;

            var startTable = new MutableTable<int>(rowCount, columnCount);

            for (int r = 0; r != rowCount; ++r)
                for (int c = 0; c != columnCount; ++c)
                {
                    startTable.Set(r, c, instructions.Count);

                    var action = table.Get(r, c);
                    if (action.Operation == ParserOperation.Conflict)
                    {
                        var conflict = parserTable.Conflicts[action.Argument];
                        int forkPos = instructions.Count;

                        foreach (var conflictAction in conflict.Actions.Skip(1))
                        {
                            AddForkInstructionPlaceholder(instructions);
                        }

                        bool first = true;
                        foreach (var conflictAction in conflict.Actions)
                        {
                            if (first)
                            {
                                first = false;
                            }
                            else
                            {
                                instructions[forkPos++] = new ParserInstruction
                                {
                                    Operation   = ParserOperation.Fork,
                                    Argument = instructions.Count
                                };
                            }

                            CompileTransition(instructions, conflictAction);
                        }
                    }
                    else
                    {
                        CompileTransition(instructions, action);
                    }
                }

            this.Instructions = instructions.ToArray();
            this.StartTable   = startTable;
        }

        private static void AddForkInstructionPlaceholder(List<ParserInstruction> instructions)
        {
            instructions.Add(ParserInstruction.InternalErrorAction);
        }

        private static void CompileTransition(List<ParserInstruction> instructions, ParserInstruction action)
        {
            instructions.Add(action);
            switch (action.Operation)
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

        public ITable<int>    StartTable   { get; }
    }
}
