using IronText.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static IronText.Misc.ObjectUtils;
using System.Collections;
using IronText.Common;
using System.Diagnostics;

namespace IronText.Runtime.RIGLR.GraphStructuredStack
{
    class MergeIndex<T>
    {
        private readonly Dictionary<long, T> index = new Dictionary<long,T>();

        public MergeIndex()
        {
        }

        public void Set(int leftmostLayer, int outcome, T value)
        {
            index[Key(leftmostLayer, outcome)] = value;
        }

        public bool Contains(int leftmostLayer, int token)
        {
            return index.ContainsKey(Key(leftmostLayer, token));
        }

        public bool TryGet(int leftmostLayer, int token, out T outcome)
        {
            return index.TryGetValue(Key(leftmostLayer, token), out outcome);
        }

        public void Clear()
        {
            index.Clear();
        }

        static long Key(int leftmostLayer, int token) => leftmostLayer << 16 | token;
    }

    class ProcessBackLink<T> : Ambiguous<ProcessBackLink<T>>
    {
        public ProcessBackLink(CallStackNode<T> prior, ProcessData<T> pending)
        {
            this.Prior   = prior;
            this.Pending = pending;
        }

        public ProcessData<T>  Pending { get; }

        public CallStackNode<T>    Prior   { get; }
    }

    class ProcessNodeLookup<T>
    {
    }

    class CallStackNode<T>
    {
        public CallStackNode(int state, CallStackNode<T> prior, Process<T> pending)
            : this(state, new ProcessBackLink<T>(prior, pending.ReductionData))
        {
        }

        public CallStackNode(int state, ProcessBackLink<T> backLink)
        {
            this.State    = state;
            this.BackLink = backLink;
        }

        public int                State    { get; }

        public ProcessBackLink<T> BackLink { get; private set; }

        public ProcessBackLink<T> LinkPrior(CallStackNode<T> node, ProcessData<T> pending)
        {
            int priorState = node.State;

            if (BackLink.AllAlternatives().Any(l => l.Prior.State == priorState && l.Pending.IsEquivalentTo(pending)))
            {
                return null;
            }

            var result = new ProcessBackLink<T>(node, pending);
            BackLink = BackLink.Alternate(result);
            return result;
        }
    }

    static class ReductionNodeExtensions
    {
        public static ProcessData<T> GetAtDepth<T>(this ProcessData<T> @this, int depth)
        {
            var result = @this;
            while (0 != depth--)
            {
                result = result.PriorData;
            }

            return result;
        }

        private static ProcessData<T> Tail<T>(this ProcessData<T> @this)
        {
            var result = @this;
            if (result != null)
            {
                while (result.PriorData != null)
                {
                    result = result.PriorData;
                }
            }

            return result;
        }

        public static bool IsEquivalentTo<T>(
            this ProcessData<T> @this,
            ProcessData<T> other)
        {
            return ReferenceEquals(@this, other)
                || (
                    @this != null
                    && other != null
                    && Equals(@this.Value, other.Value));
        }

    }

    class ProcessData<T> : IStackLookback<T>
    {
        public static ProcessData<T> Null => null;

        public ProcessData(
            int state,
            T value,
            ProcessData<T> priorData,
            int leftmostLayer)
        {
            State         = state;
            Value         = value;
            LeftmostLayer = leftmostLayer;
            PriorData     = priorData;
        }

        public int              State         { get; }
        public ProcessData<T>   PriorData     { get; protected set; }
        public int              LeftmostLayer { get; }
        public T                Value         { get; }

        public int GetState(int backOffset)
        {
            throw new NotImplementedException("TODO: remove");
        }

        T IStackLookback<T>.GetNodeAt(int backOffset) => this.GetAtDepth(backOffset - 1).Value;

        public bool Equals(ProcessData<T> other)
        {
            return other != null
                && Equals(Value, other.Value)
                && Equals(PriorData, other.PriorData);
        }

        private static bool Equals(ProcessData<T> x, ProcessData<T> y)
        {
            return x == null ? y == null : x.Equals(y);
        }

        public override bool Equals(object obj) =>
            Equals(obj as ProcessData<T>);

        public override int GetHashCode() =>
            Value?.GetHashCode() ?? 0;
    }

    class Process<T> : IEquatable<Process<T>>
    {
        public Process(
            int state,
            T value,
            ProcessData<T> priorReductionData,
            int leftmostLayer,
            CallStackNode<T> callStack)
            : this(
                state,
                new ProcessData<T>(state, value, priorReductionData, leftmostLayer),
                callStack)
        {
        }

        public Process(
            int state,
            ProcessData<T> reductionData,
            CallStackNode<T> callStack)
        {
            InstructionState = state;
            ReductionData = reductionData;
            CallStack     = callStack;
        }

        public Process(int state, CallStackNode<T> callStack)
            : this(
                  state,
                  default(T),
                  ProcessData<T>.Null,
                  0,
                  callStack)
        {
        }

        public int              InstructionState { get; }

        public ProcessData<T>   ReductionData    { get; }

        public CallStackNode<T> CallStack        { get; }

        public Process<T> PopAlong(ProcessBackLink<T> backLink)
        {
            return new Process<T>(
                CallStack.State,
                ReductionData.Value,
                backLink.Pending,
                ReductionData.LeftmostLayer, 
                backLink.Prior);
        }

        public bool Equals(Process<T> other)
        {
            return other != null
                && InstructionState == other.InstructionState
                && base.Equals(other)
                && CallStack == other.CallStack;
        }

        public override bool Equals(object obj) => Equals(obj as Process<T>);

        public override int GetHashCode() => InstructionState ^ CallStack.GetHashCode();
    }

    /// <summary>
    /// Graph of stacks AKA RCG.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class ProcessStackGraph<T>
    {
        private readonly Dictionary<int, Record> lookupItems = new Dictionary<int, Record>();

        public void Clear()
        {
            lookupItems.Clear();
        }

        public bool TryGetPushed(int state, out CallStackNode<T> node, out List<Process<T>> popped)
        {
            Record record;

            if (lookupItems.TryGetValue(state, out record))
            {
                node      = record.Node;
                popped = record.Popped;
                return true;
            }

            node   = null;
            popped = null;

            return false;
        }

        public CallStackNode<T> Push(
            int toState,
            CallStackNode<T> prior,
            Process<T> pending)
        {
            CallStackNode<T> result;
            Record record;
            if (lookupItems.TryGetValue(toState, out record))
            {
                result = record.Node;
                result.LinkPrior(prior, pending.ReductionData);
            }
            else
            {
                result = new CallStackNode<T>(toState, prior, pending);
                lookupItems.Add(result.State, new Record(result));
            }

            return result;
        }

        public IEnumerable<Process<T>> Pop(Process<T> pending)
        {
            CallStackNode<T> callStack = pending.CallStack;
            if (lookupItems.ContainsKey(callStack.State))
            {
                Debug.Assert(!lookupItems[callStack.State].Popped.Contains(pending));
                lookupItems[callStack.State].Popped.Add(pending);
            }

            return callStack.BackLink
                .AllAlternatives()
                .Select(pending.PopAlong);
        }

        struct Record
        {
            public Record(CallStackNode<T> node)
            {
                Node   = node;
                Popped = new List<Process<T>>();
            }

            public readonly CallStackNode<T>         Node;
            public readonly List<Process<T>> Popped;
        }
    }

    class ProcessCollection<T>
    {
        private readonly List<Process<T>> items = new List<Process<T>>();
        private int consumedCount;

        private readonly ProcessStackGraph<T> stackGraph = new ProcessStackGraph<T>();

        public ProcessCollection()
        {
        }

        public bool IsEmpty => items.Count == 0;

        public bool HasItemsToConsume => consumedCount != Count;

        public int Count => items.Count;

        public Process<T> this[int index] => items[index];

        public IEnumerable<Process<T>> Consume()
        {
            while (consumedCount != items.Count)
            {
                yield return items[consumedCount++];
            }
        }

        public Process<T> Add(Process<T> process)
        {
            var existing = Find(process);
            if (existing == null)
            {
                items.Add(process);
                return null;
            }

            return existing;
        }

        private Process<T> Find(Process<T> process)
        {
            return items.Find(p => p.InstructionState == process.InstructionState
                                && p.CallStack == process.CallStack
                                && Equals(p.ReductionData, process.ReductionData));
        }

        private bool Contains(Process<T> process) => Find(process) != null;

        private bool IsNew(Process<T> process) => Find(process) == null;

        public void PushGoto(Process<T> process, int pushState, int nextState)
        {
            CallStackNode<T> pushNode;
            List<Process<T>> pendingsPrefixingPushNode;

            if (stackGraph.TryGetPushed(pushState, out pushNode, out pendingsPrefixingPushNode))
            {
                // Note: `pushState` already present in a stack graph means that we have already
                // started parsing a non-term and there is no reason to do
                // it again by adding `nextState` process. It is easy to prove because
                // newly created `nextState` process would have the same parameters as
                // a one added when pushing `pushState` for the first time:
                //   - stateID (= nextState)
                //   - reductionData (= empty),
                //   - CallStack (= `pushNode`).
                // The only thing which potentially can be different is the history 
                // of non-term parsing i.e. call of this non-term from a different place.
                // This is represented by adding extra link in a stack-graph and by a
                // subseqent popping along this back link in case when pop already have 
                // been processed:

                var newBackLink = pushNode.LinkPrior(process.CallStack, process.ReductionData);
                if (newBackLink != null)
                {
                    // Fork re-popped along the new backlink
                    foreach (var popedPending in pendingsPrefixingPushNode)
                    {
                        // Schedule pop again with different top-part of pending
                        Add(popedPending.PopAlong(newBackLink));
                    }
                }
            }
            else
            {
                // Pushing `pushState` for the first time means that we haven't started to 
                // parse particular LL non-term before.

                pushNode = stackGraph.Push(pushState, process.CallStack, process);

                Add(new Process<T>(nextState, pushNode));
            }
        }

        /// <summary>
        /// Add popped-processes to stack for processing.
        /// </summary>
        /// <param name="process"></param>
        /// <returns>popped-to processes which already exists</returns>
        public void Pop(Process<T> process)
        {
            foreach (var p in stackGraph.Pop(process))
            {
                Add(p);
            }
        }

        public void Clear()
        {
            consumedCount = 0;
            stackGraph.Clear();
            items.Clear();
        }
    }

    class RiGss<T>
    {
        private ProcessCollection<T> _current = new ProcessCollection<T>();
        private ProcessCollection<T> _pending = new ProcessCollection<T>();

        public ProcessCollection<T> Current => _current;
        public ProcessCollection<T> Pending => _pending;

        public void Next()
        {
            Swap(ref _current, ref _pending);

            _pending.Clear();
        }
    }
}
