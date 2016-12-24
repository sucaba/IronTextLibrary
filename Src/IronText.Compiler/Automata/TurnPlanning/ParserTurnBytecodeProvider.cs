
using IronText.Algorithm;
using IronText.Collections;
using IronText.Common;
using IronText.Runtime;
using System.Collections.Generic;
using System;
using System.Linq;

namespace IronText.Automata.TurnPlanning
{
    class ParserPlanBytecodeProvider
    {
        private const int SharedFailurePos = 0;
        private readonly List<ParserInstruction> instructions;
        private readonly Indexer<ShrodingerTokenDfaState> indexer;

        public ParserPlanBytecodeProvider(
            ShrodingerTokenDfaProvider       dfa,
            Indexer<ShrodingerTokenDfaState> indexer,
            TokenSetProvider                 tokenSetProvider)
        {
            this.indexer = indexer;

            this.instructions = new List<ParserInstruction>();

            CompileSharedFailureAction();

            int stateCount = dfa.States.Length;
            int tokenCount = tokenSetProvider.TokenSet.MaxValue + 1;

            var startTable = new MutableTable<int>(stateCount, tokenCount);

            foreach (var state in dfa.States)
            {
                int stateIndex = indexer.Get(state);

                for (int token = 0; token != tokenCount; ++token)
                {
                    var decision = state.GetDecision(token);
                    if (decision == ShrodingerTokenDecision.NoAlternatives)
                    {
                        startTable.Set(stateIndex, token, SharedFailurePos);
                    }
                    else
                    {
                        startTable.Set(stateIndex, token, NextInstructionPos);
                        CompileAmbiguousDecision(decision);
                    }
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

        private void CompileAmbiguousDecision(ShrodingerTokenDecision decision)
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

        private void CompileDecision(ShrodingerTokenDecision decision)
        {
            CompileTurn(
                (dynamic)decision.Turn,
                indexer.Get(decision.NextState));

            CompileBranchEnd();
        }

        private void CompileTurn(ReductionTurn turn, int nextState)
        {
            instructions.Add(ParserInstruction.Reduce(turn.ProductionId));
            instructions.Add(ParserInstruction.ForceState(nextState));
        }

        private void CompileTurn(ReturnTurn turn, int nextState)
        {
            instructions.Add(ParserInstruction.Return(turn.ProducedToken));
        }

        private void CompileTurn(InputConsumptionTurn turn, int nextState)
        {
            instructions.Add(ParserInstruction.Shift(nextState));
        }

        private void CompileTurn(AcceptanceTurn turn, int nextState)
        {
            instructions.Add(ParserInstruction.AcceptAction);
        }

        private void CompileBranchEnd()
        {
            switch (instructions.Last().Operation)
            {
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
