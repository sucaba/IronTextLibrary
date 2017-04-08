using IronText.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static IronText.Misc.ObjectUtils;
using System.Collections;
using IronText.Common;

namespace IronText.Runtime.RIGLR.GraphStructuredStack
{
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
                @this.Token,
                @this.Value,
                @this.Prior.DeepClone(tail),
                (tail ?? @this).Tail().LeftmostLayer);
        }
    }

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

    class ReductionNode<T> : IStackLookback<T>
    {
        public static ReductionNode<T> Null => null;

        public ReductionNode(int token, T value, ReductionNode<T> prior, int leftmostLayer)
        {
            Token = token;
            Value = value;
            Prior = prior;
            LeftmostLayer = leftmostLayer;
        }

        // TODO: Why it is needed?
        public int              Token    { get; }

        public T                Value    { get; }

        public ReductionNode<T> Prior    { get; }

        public int              LeftmostLayer    { get; }

        public int GetState(int backOffset)
        {
            throw new NotImplementedException("TODO: remove");
        }

        T IStackLookback<T>.GetNodeAt(int backOffset) => this.GetAtDepth(backOffset - 1).Value;
    }

    class ProcessBackLink<T> : Ambiguous<ProcessBackLink<T>>
    {
        public ProcessBackLink(ProcessNode<T> prior, ReductionNode<T> pending)
        {
            this.Prior   = prior;
            this.Pending = pending;
        }

        public ReductionNode<T>  Pending { get; }

        public ProcessNode<T>    Prior   { get; }
    }

    class ProcessNodeLookup<T>
    {
    }

    class ProcessNode<T> : IEquatable<ProcessNode<T>>
    {
        public ProcessNode(int state, ProcessNode<T> prior, ReductionNode<T> pending)
            : this(state, new ProcessBackLink<T>(prior, pending))
        {
        }

        public ProcessNode(int state, ProcessBackLink<T> backLink)
        {
            this.State    = state;
            this.BackLink = backLink;
        }

        public int                State    { get; }

        public ProcessBackLink<T> BackLink { get; private set; }

        public bool Equals(ProcessNode<T> other) => other != null && other.State == State;

        public override bool Equals(object obj) => Equals(obj as ProcessNode<T>);

        public override int GetHashCode() => State;

        public ProcessBackLink<T> LinkPrior(ProcessNode<T> node, ReductionNode<T> pending)
        {
            int priorState = node.State;

            if (BackLink.AllAlternatives().Any(l => l.Prior.State == priorState))
            {
                return null;
            }

            var result = new ProcessBackLink<T>(node, pending);
            BackLink = BackLink.Alternate(result);
            return result;
        }
    }

    class Process<T> : IEquatable<Process<T>>
    {
        public Process(int state, ReductionNode<T> pending, ProcessNode<T> callStack)
        {
            State     = state;
            Pending   = pending;
            CallStack = callStack;
        }

        public int               State     { get; }
        public ReductionNode<T>  Pending   { get; set; }
        public ProcessNode<T>    CallStack { get; }

        public bool Equals(Process<T> other)
        {
            return other != null
                && State == other.State
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

        public bool TryGetPopped(int state, out ProcessNode<T> node, out List<ReductionNode<T>> popped)
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

        public ProcessNode<T> Push(
            int toState,
            ProcessNode<T> prior,
            ReductionNode<T> pending)
        {
            ProcessNode<T> result;
            Record record;
            if (lookupItems.TryGetValue(toState, out record))
            {
                result = record.Node;
                result.LinkPrior(prior, pending);
            }
            else
            {
                result = new ProcessNode<T>(toState, prior, pending);
                lookupItems.Add(result.State, new Record(result));
            }

            return result;
        }

        public IEnumerable<Process<T>> Pop(ProcessNode<T> callStack, ReductionNode<T> pending)
        {
            if (lookupItems.ContainsKey(callStack.State))
            {
                lookupItems[callStack.State].Popped.Add(pending);
            }

            return callStack.BackLink
                .AllAlternatives()
                .Select(backLink =>
                    new Process<T>(
                        callStack.State,
                        pending.ImmutableAppend(backLink.Pending),
                        backLink.Prior));
        }

        struct Record
        {
            public Record(ProcessNode<T> node)
            {
                Node   = node;
                Popped = new List<ReductionNode<T>>();
            }

            public readonly ProcessNode<T>         Node;
            public readonly List<ReductionNode<T>> Popped;
        }
    }

    class ProcessCollection<T> : IEnumerable<Process<T>>
    {
        private readonly List<Process<T>> items = new List<Process<T>>();

        private readonly ProcessStackGraph<T> stackGraph = new ProcessStackGraph<T>();

        public ProcessCollection()
        {
        }

        public bool IsEmpty => items.Count == 0;

        public int Count => items.Count;

        public Process<T> this[int index] => items[index];

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
                                && p.Pending == process.Pending);
        }

        private bool Contains(Process<T> process) => Find(process) != null;

        private bool IsNew(Process<T> process) => Find(process) == null;

        public void PushGoto(Process<T> process, int pushState, int nextState)
        {
            ProcessNode<T> pushNode;
            List<ReductionNode<T>> poppedPending;

            if (stackGraph.TryGetPopped(pushState, out pushNode, out poppedPending))
            {
                ProcessBackLink<T> newBackLink = pushNode.LinkPrior(
                                                    process.CallStack,
                                                    process.Pending);
                if (newBackLink != null)
                {
                    // Fork re-popped along the new link
                    foreach (var pop in poppedPending)
                    {
                        // Schedule pop again with different top-part of pending
                        Add(new Process<T>(
                                pushNode.State,
                                process.Pending.ImmutableAppend(pop),
                                newBackLink.Prior));
                    }
                }
            }
            else
            {
                pushNode = stackGraph.Push(pushState, process.CallStack, process.Pending);

                Add(new Process<T>(
                        nextState,
                        ReductionNode<T>.Null,
                        pushNode));
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
            stackGraph.Clear();
            items.Clear();
        }

        public IEnumerator<Process<T>> GetEnumerator()
        {
            return items.EnumerateGrowable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
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
