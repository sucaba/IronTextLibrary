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

            this.stack.Current.Add(new Process<T>(0, null, null));

            this.reductionQueue = new ReductionQueueWithPriority<T>(tokenComplexity);
        }

        public IReceiver<Message> Next(Message message)
        {
            lastLocation = message.Location;
            if (message.IsAmbiguous)
            {
                throw new NotImplementedException();
            }

            reductionQueue.Clear();

            MessageData alternateInput = message;

            var term = producer.CreateLeaf(message, alternateInput);

            bool accepted = false;

            foreach (var process in stack.Current)
            {
                int start = actionTable(process.State, alternateInput.Token);

                if (ProcessPosition(message, alternateInput, term, process, start))
                {
                    accepted = true;
                }
            }

            do
            {
                int countBefore = stack.Current.Count();

                Reduction<T> r;
                if (!reductionQueue.TryDequeue(out r))
                {
                    break;
                }

                ProcessReduction(r);

                if (countBefore == stack.Current.Count)
                {
                    break;
                }

                for (int i = countBefore;  i != stack.Current.Count; ++i)
                {
                    Process<T> process = stack.Current[i];

                    int start = actionTable(process.State, alternateInput.Token);

                    if (ProcessPosition(message, alternateInput, term, process, start))
                    {
                        accepted = true;
                    }
                }
            }
            while (true);

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

        bool ProcessPosition(Message message, MessageData alternateInput, T term, Process<T> process, int start)
        {
            bool result = false;

            while (true)
            {
                ParserInstruction instruction = grammar.Instructions[start];

                switch (instruction.Operation)
                {
                    case ParserOperation.Accept:
                        result = true;
                        break;
                    case ParserOperation.Fail:
                        break;
                    case ParserOperation.Shift:
                        stack.Pending.Add(
                            new Process<T>(
                                instruction.State,
                                new ReductionNode<T>(alternateInput.Token, term, process.Pending, currentLayer + 1),
                                process.CallStack));
                        break;
                    case ParserOperation.ReduceGoto:
                        QueueReduction(
                            process,
                            grammar.Productions[instruction.Production],
                            instruction.Argument2);
                        break;
                    case ParserOperation.Pop:
                        foreach (var existing in stack.Current.Pop(process))
                        {
                            var bottom = existing.Pending.Prior;

                            var merged = producer.Merge(
                                existing.Pending.Value,
                                process.Pending.Value,
                                bottom);
                            existing.Pending = new ReductionNode<T>(
                                existing.Pending.Token,
                                merged,
                                bottom,
                                currentLayer);
                        }
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
            int               nextState)
        {
            int leftmostLayer = process.Pending.GetAtDepth(production.InputLength)?.Layer ?? 0;
            reductionQueue.Enqueue(new Reduction<T>(process, production, nextState, leftmostLayer));
        }

        private void ProcessReduction(Reduction<T> reduction)
        {
            var value = producer.CreateBranch(reduction.Production, reduction.Process.Pending);
            var bottom = reduction.Process.Pending.GetAtDepth(reduction.Production.InputLength);
            var existing = stack.Current.Add(
                new Process<T>(
                    reduction.NextState,
                    new ReductionNode<T>(reduction.Production.Outcome, value, bottom, currentLayer),
                    reduction.Process.CallStack));

            Debug.Assert(existing == null);
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
