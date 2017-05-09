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
        public ProcessBackLink(CallStackNode<T> prior, ReductionNode<T> pending)
        {
            this.Prior   = prior;
            this.Pending = pending;
        }

        public ReductionNode<T>  Pending { get; }

        public CallStackNode<T>    Prior   { get; }
    }

    class ProcessNodeLookup<T>
    {
    }

    class CallStackNode<T>
    {
        public CallStackNode(int state, CallStackNode<T> prior, ReductionNode<T> pending)
            : this(state, new ProcessBackLink<T>(prior, pending))
        {
        }

        public CallStackNode(int state, ProcessBackLink<T> backLink)
        {
            this.State    = state;
            this.BackLink = backLink;
        }

        public int                State    { get; }

        public ProcessBackLink<T> BackLink { get; private set; }

        public ProcessBackLink<T> LinkPrior(CallStackNode<T> node, ReductionNode<T> pending)
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

        public Process<T> PopAlong(
            ProcessBackLink<T> backLink,
            ReductionNode<T>   pending)
        {
            return new Process<T>(
                State,
                pending.ImmutableAppend(backLink.Pending),
                backLink.Prior);
        }
    }

    static class ReductionNodeExtensions
    {
        public static ReductionNode<T> GetAtDepth<T>(this ReductionNode<T> @this, int depth)
        {
            var result = @this;
            while (0 != depth--)
            {
                result = result.Prior;
            }

            return result;
        }

        public static ReductionNode<T> ImmutableAppend<T>(this ReductionNode<T> @this, ReductionNode<T> nodes)
        {
            return @this.DeepClone(tail: nodes);
        }

        private static ReductionNode<T> Tail<T>(this ReductionNode<T> @this)
        {
            var result = @this;
            if (result != null)
            {
                while (result.Prior != null)
                {
                    result = result.Prior;
                }
            }

            return result;
        }

        public static ReductionNode<T> DeepClone<T>(
            this ReductionNode<T> @this,
            ReductionNode<T> tail = null)
        {
            if (@this == null)
            {
                return tail;
            }

            return new ReductionNode<T>(
                @this.Value,
                @this.Prior.DeepClone(tail),
                (tail ?? @this).Tail().LeftmostLayer);
        }

        public static bool IsEquivalentTo<T>(
            this ReductionNode<T> @this,
            ReductionNode<T> other)
        {
            return ReferenceEquals(@this, other)
                || (
                    @this != null
                    && other != null
                    && Equals(@this.Value, other.Value));
        }

    }

    class ReductionNode<T> : IStackLookback<T>
    {
        public static ReductionNode<T> Null => null;

        public ReductionNode(T value, ReductionNode<T> prior, int leftmostLayer)
        {
            Value = value;
            Prior = prior;
            LeftmostLayer = leftmostLayer;
        }

        public T                Value    { get; }

        public ReductionNode<T> Prior    { get; }

        public int              LeftmostLayer    { get; }

        public int GetState(int backOffset)
        {
            throw new NotImplementedException("TODO: remove");
        }

        T IStackLookback<T>.GetNodeAt(int backOffset) => this.GetAtDepth(backOffset - 1).Value;
    }


    class Process<T> : IEquatable<Process<T>>
    {
        public Process(
            int state,
            T value,
            ReductionNode<T> prior,
            int leftmostLayer,
            CallStackNode<T> callStack)
            : this(state, new ReductionNode<T>(value, prior, leftmostLayer), callStack)
        {
        }

        public Process(int state, CallStackNode<T> callStack)
            : this(
                  state,
                  ReductionNode<T>.Null,
                  callStack)
        {
        }

        public Process(int state, ReductionNode<T> pending, CallStackNode<T> callStack)
        {
            State     = state;
            Pending   = pending;
            CallStack = callStack;
        }

        public int               State     { get; }
        public ReductionNode<T>  Pending   { get; set; }
        public CallStackNode<T>    CallStack { get; }

        public bool Equals(Process<T> other)
        {
            return other != null
                && State == other.State
                && Pending.IsEquivalentTo(other.Pending)
                && CallStack == other.CallStack;
        }

        public override bool Equals(object obj) => Equals(obj as Process<T>);

        public override int GetHashCode() => State ^ CallStack.GetHashCode();
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

        public bool TryGetPushed(int state, out CallStackNode<T> node, out List<ReductionNode<T>> popped)
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
            ReductionNode<T> pending)
        {
            CallStackNode<T> result;
            Record record;
            if (lookupItems.TryGetValue(toState, out record))
            {
                result = record.Node;
                result.LinkPrior(prior, pending);
            }
            else
            {
                result = new CallStackNode<T>(toState, prior, pending);
                lookupItems.Add(result.State, new Record(result));
            }

            return result;
        }

        public IEnumerable<Process<T>> Pop(CallStackNode<T> callStack, ReductionNode<T> pending)
        {
            if (lookupItems.ContainsKey(callStack.State))
            {
                Debug.Assert(!lookupItems[callStack.State].Popped.Contains(pending));
                lookupItems[callStack.State].Popped.Add(pending);
            }

            return callStack.BackLink
                .AllAlternatives()
                .Select(backLink => callStack.PopAlong(backLink, pending));
        }

        struct Record
        {
            public Record(CallStackNode<T> node)
            {
                Node   = node;
                Popped = new List<ReductionNode<T>>();
            }

            public readonly CallStackNode<T>         Node;
            public readonly List<ReductionNode<T>> Popped;
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
            return items.Find(p => p.State == process.State
                                && p.CallStack == process.CallStack
                                && p.Pending.IsEquivalentTo(process.Pending));
        }

        private bool Contains(Process<T> process) => Find(process) != null;

        private bool IsNew(Process<T> process) => Find(process) == null;

        public void PushGoto(Process<T> process, int pushState, int nextState)
        {
            CallStackNode<T> pushNode;
            List<ReductionNode<T>> pendingsPrefixingPushNode;

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

                var newBackLink = pushNode.LinkPrior(process.CallStack, process.Pending);
                if (newBackLink != null)
                {
                    // Fork re-popped along the new backlink
                    foreach (var popedPending in pendingsPrefixingPushNode)
                    {
                        // Schedule pop again with different top-part of pending
                        Add(pushNode.PopAlong(newBackLink, popedPending));
                    }
                }
            }
            else
            {
                // Pushing `pushState` for the first time means that we haven't started to 
                // parse particular LL non-term before. Doing it now:

                pushNode = stackGraph.Push(pushState, process.CallStack, process.Pending);

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
            foreach (var p in stackGraph.Pop(process.CallStack, process.Pending))
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
