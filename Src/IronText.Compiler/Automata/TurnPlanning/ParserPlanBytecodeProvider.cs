
using IronText.Algorithm;
using IronText.Collections;
using IronText.Common;
using IronText.Runtime;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Diagnostics;

namespace IronText.Automata.TurnPlanning
{
    class ParserPlanBytecodeProvider : IParserBytecodeProvider
    {
        public const int DispatchOffset = 2;

        private const int SharedFailurePos = 0;
        private static ParserInstruction InstructionStub => ParserInstruction.InternalErrorAction;

        private readonly List<ParserInstruction> instructions;
        private readonly StateToPos stateToPos;

        public ParserInstruction[] Instructions   { get; }

        public ITable<int>         StartTable     { get; }


        public ParserPlanBytecodeProvider(
            ShrodingerTokenDfaProvider  dfa,
            StateToPos                  stateToPos,
            TokenSetProvider            tokenSetProvider)
        {
            this.stateToPos = stateToPos;

            this.instructions = new List<ParserInstruction>();

            CompileSharedFailureAction();

            Debug.Assert(DispatchOffset == NextInstructionPos);

            instructions.AddRange(
                Enumerable
                .Repeat(InstructionStub, dfa.States.Length));
            foreach (var state in dfa.States)
            {
                int pos = stateToPos.Get(state);
                instructions[pos] = ParserInstruction.Dispatch(pos);
            }

            this.StartTable = CompileInputProcessing(dfa, tokenSetProvider);

            this.Instructions = instructions.ToArray();
            instructions.Clear();
        }

        private MutableTable<int> CompileInputProcessing(ShrodingerTokenDfaProvider dfa, TokenSetProvider tokenSetProvider)
        {
            int stateCount = dfa.States.Length;
            int tokenCount = tokenSetProvider.TokenSet.MaxValue + 1;

            var startTable = new MutableTable<int>(stateCount + DispatchOffset, tokenCount);

            foreach (var fromState in dfa.States)
            {
                int statePos = stateToPos.Get(fromState);

                for (int token = 0; token != tokenCount; ++token)
                {
                    var decision = fromState.GetDecision(token);
                    if (decision == ShrodingerTokenDecision.NoAlternatives)
                    {
                        startTable.Set(statePos, token, SharedFailurePos);
                    }
                    else
                    {
                        startTable.Set(statePos, token, NextInstructionPos);
                        CompileAmbiguousDecision(fromState, decision);
                    }
                }
            }

            return startTable;
        }

        private int NextInstructionPos => instructions.Count;

        private void CompileSharedFailureAction()
        {
            instructions.Add(ParserInstruction.FailAction);
            CompileBranchEnd();
        }

        private void CompileAmbiguousDecision(
            ShrodingerTokenDfaState fromState,
            ShrodingerTokenDecision decision)
        {
            int forkInstructionPos = NextInstructionPos;

            foreach (var other in decision.OtherAlternatives())
            {
                instructions.Add(InstructionStub);
            }

            CompileDecision(fromState, decision);

            foreach (var other in decision.OtherAlternatives())
            {
                instructions[forkInstructionPos++] = ParserInstruction.Fork(NextInstructionPos);
                CompileDecision(fromState, other);
            }
        }

        private void CompileDecision(
            ShrodingerTokenDfaState fromState,
            ShrodingerTokenDecision decision)
        {
            CompileTurn(
                fromState,
                (dynamic)decision.Turn,
                stateToPos.Get(decision.NextState));

            CompileBranchEnd();
        }

        private void CompileTurn(ShrodingerTokenDfaState fromState, TopDownReductionTurn turn, int nextPos)
        {
            instructions.Add(ParserInstruction.ReduceGoto(turn.ProductionId, nextPos));
        }

        private void CompileTurn(ShrodingerTokenDfaState fromState, BottomUpReductionTurn turn, int nextPos)
        {
            instructions.Add(ParserInstruction.Reduce(turn.ProductionId));
        }

        private void CompileTurn(ShrodingerTokenDfaState fromState, EnterTurn turn, int nextPos)
        {
            int nonTerm = turn.ProducedToken;

            var returnState = fromState.GetDecision(nonTerm)
                .Resolve(d => d.Turn.Consumes(nonTerm))
                .NextState;

            instructions.Add(
                ParserInstruction.PushGoto(
                    stateToPos.Get(returnState),
                    nextPos));
        }

        private void CompileTurn(ShrodingerTokenDfaState fromState, ReturnTurn turn, int nextPos)
        {
            instructions.Add(ParserInstruction.Pop);
        }

        private void CompileTurn(ShrodingerTokenDfaState fromState, InputConsumptionTurn turn, int nextPos)
        {
            instructions.Add(ParserInstruction.Shift(nextPos));
        }

        private void CompileTurn(ShrodingerTokenDfaState fromState, AcceptanceTurn turn, int nextPos)
        {
            instructions.Add(ParserInstruction.AcceptAction);
        }

        private void CompileBranchEnd()
        {
            // safety instruction to avoid invalid instruction access
            instructions.Add(ParserInstruction.InternalErrorAction);
        }

        internal class StateToPos
        {
            private readonly Indexer<ShrodingerTokenDfaState> indexer;

            public StateToPos(Indexer<ShrodingerTokenDfaState> indexer)
            {
                this.indexer = indexer;
            }

            public int Get(ShrodingerTokenDfaState state)
            {
                return DispatchOffset + indexer.Get(state);
            }
        }
    }
}
