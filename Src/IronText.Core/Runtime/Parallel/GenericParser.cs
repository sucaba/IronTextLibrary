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
            this.grammar     = grammar;
            this.producer    = producer;
            this.actionTable = actionTable;
            this.logging     = logging;

            this.stack.Current.Add(new Process<T>(0, null));

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

            if (stack.Pending.IsEmpty)
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

        bool ProcessState(Message message, MessageData alternateInput, T term, Process<T> process)
        {
            int start;
            if (process.State < 0)
            {
                start = -process.State;
            }
            else
            {
                start = GetActionStart(process.State, alternateInput.Token);
            }

            return ProcessPosition(message, alternateInput, term, process, start);
        }

        bool ProcessPosition(Message message, MessageData alternateInput, T term, Process<T> process, int start)
        {
            bool result = false;

            while (true)
            {
                ParserInstruction instruction = grammar.Instructions[start];
                diagnostics.Action(instruction);

                switch (instruction.Operation)
                {
                    case ParserOperation.Accept:
                        result = true;
                        producer.Result = process.Value;
                        break;
                    case ParserOperation.Fail:
                        break;
                    case ParserOperation.Shift:
                        stack.Pending.Add(
                            new Process<T>(
                                instruction.State,
                                term,
                                process,
                                currentLayer,
                                process.CallStack));
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
                        stack.Current.Pop(process);
                        break;
                    case ParserOperation.PushGoto:
                        stack.Current.PushGoto(process, instruction.PushState, instruction.State);
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
                : process.GetAtDepth(production.InputLength - 1).LeftmostLayer;
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

            var bottom = reduction.Process.GetAtDepth(reduction.Production.InputLength);
            var currentValue = producer.CreateBranch(reduction.Production, reduction.Process);

            T mergedValue;
            if (N.TryGet(reduction.LeftmostLayer, reduction.Production.Outcome, out mergedValue))
            {
                mergedValue = producer.Merge(mergedValue, currentValue, bottom);
            }
            else
            {
                mergedValue = currentValue;
            }

            foreach (var duplicate in reductions.Skip(1))
            {
                diagnostics.ProcessReduction(duplicate);
                var duplicateBottom = duplicate.Process.GetAtDepth(duplicate.Production.InputLength);
                // Note: following does not work after joining Process and ReductionNode
                // Debug.Assert(
                //     ReferenceEquals(bottom, duplicateBottom),
                //     "Assumption failed: different reduction alternatives have same bottom");
                currentValue = producer.CreateBranch(duplicate.Production, duplicate.Process);

                mergedValue = producer.Merge(mergedValue, currentValue, duplicateBottom);
            }

            N.Set(reduction.LeftmostLayer, reduction.Production.Outcome, mergedValue);

            foreach (var r in reductions)
            {
                var duplicateBottom = r.Process.GetAtDepth(r.Production.InputLength);

                var nextState = r.NextState;
                if (nextState < 0)
                {
                    nextState = -actionTable(r.Process.State, r.Production.Outcome);
                }

                stack.Current.Add(
                    new Process<T>(
                        nextState,
                        mergedValue,
                        duplicateBottom,
                        reduction.LeftmostLayer,
                        r.Process.CallStack));
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
