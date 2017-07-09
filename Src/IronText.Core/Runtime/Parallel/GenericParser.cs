using IronText.Collections;
using IronText.Common;
using IronText.Logging;
using IronText.Runtime.RIGLR.GraphStructuredStack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace IronText.Runtime
{
    class GenericParser<T> : IPushParser
    {
        private readonly RiGss<T> stack = new RiGss<T>();
        private readonly TransitionDelegate actionTable;
        private readonly RuntimeGrammar grammar;
        private readonly MergeIndex<T> N = new MergeIndex<T>();
        private readonly ReductionQueueWithPriority<T> reductionQueue;
        private readonly IProducer<T> producer;
        private readonly ILogging logging;
        private readonly GenericParserDiagnostics<T> diagnostics = new GenericParserDiagnostics<T>();

        private int currentLayer = 0;
        private Loc lastLocation = new Loc(1, 1, 1, 1);

        public GenericParser(
            IProducer<T>       producer,
            RuntimeGrammar     grammar,
            TransitionDelegate actionTable,
            int[]              tokenComplexity,
            ILogging           logging)
        {
            this.grammar        = grammar;
            this.producer       = producer;
            this.actionTable    = actionTable;
            this.logging        = logging;

            this.stack.Current.Init(2);

            this.reductionQueue = new ReductionQueueWithPriority<T>(tokenComplexity);
        }

        private int GetActionStart(int state, int token)
        {
            return actionTable(state, token);
        }

        public IReceiver<Message> Next(Message message)
        {
            lastLocation = message.Location;
            if (message.IsAmbiguous)
            {
                throw new NotImplementedException();
            }

            reductionQueue.Clear();
            N.Clear();

            MessageData alternateInput = message;

            diagnostics.StartInput(alternateInput);

            var term = producer.CreateLeaf(message, alternateInput);

            bool accepted = false;

            do
            {
                foreach (var process in stack.Current.Consume())
                {
                    diagnostics.SetCurrentProcess(process);

                    if (ProcessState(message, alternateInput, term, process))
                    {
                        accepted = true;
                    }
                }

                var reductionsToMerge = new List<Reduction<T>>();
                if (0 == reductionQueue.TryDequeue(reductionsToMerge))
                {
                    break;
                }

                ProcessReduction(reductionsToMerge);
            }
            while (stack.Current.HasItemsToConsume || !reductionQueue.IsEmpty);

            if (accepted)
            {
                return FinalReceiver<Message>.Instance;
            }

            if (!stack.Future.HasItemsToConsume)
            {
                logging.Write(
                    new LogEntry
                    {
                        Severity = Severity.Error,
                        Message = "Invalid syntax.",
                        Location = message.Location,
                    });
            }

            stack.Next();

            ++currentLayer;
            return this;
        }

        bool ProcessState(
            Message message,
            MessageData alternateInput,
            T term,
            Process<T> process)
        {
            return ProcessPosition(
                message,
                alternateInput,
                term,
                process,
                process.InstructionState);
        }

        bool ProcessPosition(
            Message message,
            MessageData alternateInput,
            T term,
            Process<T> process,
            int start,
            bool isShift = true)
        {
            bool result = false;

            while (true)
            {
                ParserInstruction instruction = grammar.Instructions[start];
                diagnostics.Action(instruction);

                switch (instruction.Operation)
                {
                    case ParserOperation.Dispatch:
                        Debug.Assert(start == instruction.State);
                        start = actionTable(instruction.State, alternateInput.Token);
                        diagnostics.GotoPos(start);
                        continue;
                    case ParserOperation.Accept:
                        result = true;
                        producer.Result = process.ReductionData.Value;
                        break;
                    case ParserOperation.Fail:
                        break;
                    case ParserOperation.Shift:
                        var stage = isShift ? stack.Future : stack.Current;
                        stage.EnqueueShift(process, instruction.State, term, currentLayer);
                        break;
                    case ParserOperation.ReduceGoto:
                        QueueReduction(
                            process,
                            grammar.Productions[instruction.Production],
                            instruction.Argument2);
                        break;
                    case ParserOperation.Reduce:
                        QueueReduction(
                            process,
                            grammar.Productions[instruction.Production]);
                        break;
                    case ParserOperation.Pop:
                        stack.Current.EnqueuePop(process);
                        break;
                    case ParserOperation.PushGoto:
                        stack.Current.EnqueuePushGoto(process, instruction.PushState, instruction.State);
                        break;
                    case ParserOperation.Fork:
                        if (ProcessPosition(message, alternateInput, term, process, instruction.ForkPosition))
                        {
                            result = true;
                        }
                        ++start;
                        continue;
                    default:
                        throw new NotSupportedException($"Invalid operation ${instruction.Operation}");
                }

                break;
            }

            return result;
        }

        private void QueueReduction(
            Process<T>        process,
            RuntimeProduction production,
            int               nextState = -1)
        {
            int leftmostLayer =
                production.InputLength == 0
                ? currentLayer
                : process.ReductionData.GetAtDepth(production.InputLength - 1).LeftmostLayer;
            reductionQueue.Enqueue(
                new Reduction<T>(
                    process,
                    production,
                    nextState,
                    leftmostLayer));
        }

        private void ProcessReduction(List<Reduction<T>> reductions)
        {
            var reduction = reductions.First();

            var firstBottom = reduction.Process.ReductionData.GetAtDepth(reduction.Production.InputLength);
            diagnostics.ProcessReduction(reduction);
            var currentValue = producer.CreateBranch(reduction.Production, reduction.Process.ReductionData);

            T mergedValue;
            if (N.TryGet(reduction.LeftmostLayer, reduction.Production.Outcome, out mergedValue))
            {
                mergedValue = producer.Merge(mergedValue, currentValue, firstBottom);
            }
            else
            {
                mergedValue = currentValue;
            }

            foreach (var duplicate in reductions.Skip(1))
            {
                diagnostics.ProcessReduction(duplicate);
                var duplicateBottom = duplicate.Process.ReductionData.GetAtDepth(duplicate.Production.InputLength);
                // Note: following does not work after joining Process and ReductionNode
                // Debug.Assert(
                //     ReferenceEquals(bottom, duplicateBottom),
                //     "Assumption failed: different reduction alternatives have same bottom");
                currentValue = producer.CreateBranch(duplicate.Production, duplicate.Process.ReductionData);

                mergedValue = producer.Merge(mergedValue, currentValue, duplicateBottom);
            }

            N.Set(reduction.LeftmostLayer, reduction.Production.Outcome, mergedValue);

            foreach (var r in reductions)
            {
                var bottom = r.Process.ReductionData.GetAtDepth(r.Production.InputLength);

                var nextState = r.NextState;
                if (nextState < 0)
                {
                    var input = new Message(r.Production.Outcome, null, mergedValue, Loc.Unknown);
                    var p = new Process<T>(bottom, r.Process.CallStack);
                    ProcessPosition(
                        input,
                        input,
                        mergedValue,
                        p,
                        p.InstructionState,
                        isShift: false);
                }
                else
                {
                    stack.Current.EnqueueShift(
                        r.Process.CallStack,
                        bottom,
                        nextState,
                        mergedValue,
                        reduction.LeftmostLayer);
                }
            }
        }

        public IPushParser CloneVerifier()
        {
            throw new NotImplementedException();
        }

        public IReceiver<Message> Done()
        {
            var eoi = new Message(PredefinedTokens.Eoi, null, null, lastLocation);
            return Next(eoi);
        }

        public IReceiver<Message> ForceNext(params Message[] msg)
        {
            throw new NotImplementedException();
        }
    }
}
