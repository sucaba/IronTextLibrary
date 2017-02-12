﻿using IronText.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static IronText.Misc.ObjectUtils;
using System.Collections;
using IronText.Common;

namespace IronText.Runtime.RIGLR.GraphStructuredStack
{
    class ReductionNode<T> : IStackLookback<T>
    {
        public static ReductionNode<T> Null => null;

        public ReductionNode(int token, T value, ReductionNode<T> prior)
        {
            Token = token;
            Value = value;
            Prior = prior;
        }

        // TODO: Why it is needed?
        public int              Token    { get; }

        public T                Value    { get; }

        public ReductionNode<T> Prior    { get; }

        public ReductionNode<T> GetAtDepth(int depth)
        {
            var result = this;
            while (0 != depth--)
            {
                result = result.Prior;
            }

            return result;
        }

        public int GetState(int backOffset)
        {
            throw new NotImplementedException("TODO: remove");
        }

        T IStackLookback<T>.GetNodeAt(int depth) => GetAtDepth(depth).Value;

        public ReductionNode<T> DeepClone(ReductionNode<T> tail = null)
            => DeepClone(this, tail);

        private static ReductionNode<T> DeepClone(
            ReductionNode<T> source,
            ReductionNode<T> tail = null)
        {
            if (source == null)
            {
                return tail;
            }

            return new ReductionNode<T>(
                source.Token,
                source.Value,
                DeepClone(source.Prior, tail));
        }

        public ReductionNode<T> ImmutableAppend(ReductionNode<T> nodes)
        {
            return DeepClone(tail: nodes);
        }
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
        private readonly Dictionary<int, Record> items = new Dictionary<int, Record>();

        public void RegisterPop(ProcessNode<T> destinationNode, ReductionNode<T> popPending)
        {
            items[destinationNode.State].Popped.Add(popPending);
        }

        public void RegisterPush(ProcessNode<T> destinationNode)
        {
            int state = destinationNode.State;
            var node = items[state].Node;
            items.Add(node.State, new Record(node));
        }

        public bool TryGetPopped(int state, out ProcessNode<T> node, out List<ReductionNode<T>> popped)
        {
            Record record;

            if (items.TryGetValue(state, out record))
            {
                node      = record.Node;
                popped = record.Popped;
                return true;
            }

            node   = null;
            popped = null;

            return false;
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
        public ReductionNode<T>  Pending   { get; }
        public ProcessNode<T>    CallStack { get; }

        public IEnumerable<Process<T>> ImmutablePop()
        {
            return CallStack.BackLink
                .AllAlternatives()
                .Select(backLink =>
                    new Process<T>(
                        CallStack.State,
                        Pending.ImmutableAppend(backLink.Pending),
                        backLink.Prior));
        }

        public bool Equals(Process<T> other)
        {
            return other != null
                && State == other.State
                && CallStack == other.CallStack;
        }

        public override bool Equals(object obj) => Equals(obj as Process<T>);

        public override int GetHashCode() => State ^ CallStack.GetHashCode();
    }

    class ProcessCollection<T> : IEnumerable<Process<T>>
    {
        private readonly List<Process<T>> items = new List<Process<T>>();

        public ProcessCollection()
        {
        }

        public void Add(Process<T> process)
        {
            items.Add(process);
        }

        public void AddRange(IEnumerable<Process<T>> processes)
        {
            foreach (var p in processes)
            {
                Add(p);
            }
        }

        public void Clear() { items.Clear(); }

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
