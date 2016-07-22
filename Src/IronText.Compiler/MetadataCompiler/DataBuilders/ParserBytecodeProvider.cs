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
            var instructions = new List<ParserAction>();

            var table       = parserTable.ParserActionTable;
            int rowCount    = table.RowCount;
            int columnCount = table.ColumnCount;

            var startTable = new MutableTable<int>(rowCount, columnCount);

            for (int r = 0; r != rowCount; ++r)
                for (int c = 0; c != columnCount; ++c)
                {
                    startTable.Set(r, c, instructions.Count);

                    var action = table.Get(r, c);
                    if (action.Kind == ParserActionKind.Conflict)
                    {
                        var conflict = parserTable.Conflicts[action.Value1];
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
                                instructions[forkPos++] = new ParserAction
                                {
                                    Kind   = ParserActionKind.Fork,
                                    Value1 = instructions.Count
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

        private static void AddForkInstructionPlaceholder(List<ParserAction> instructions)
        {
            instructions.Add(ParserAction.InternalErrorAction);
        }

        private static void CompileTransition(List<ParserAction> instructions, ParserAction action)
        {
            instructions.Add(action);
            switch (action.Kind)
            {
                case ParserActionKind.Resolve:
                case ParserActionKind.Reduce:
                    instructions.Add(ParserAction.ContinueAction);
                    break;
                case ParserActionKind.Shift:
                    instructions.Add(ParserAction.ExitAction);
                    break;
                default:
                    // safety instruction to avoid invalid instruction access
                    instructions.Add(ParserAction.InternalErrorAction);
                    break;
            }
        }

        public ParserAction[] Instructions { get; }

        public ITable<int>    StartTable   { get; }
    }
}
