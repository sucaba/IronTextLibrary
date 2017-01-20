using IronText.Algorithm;
using System;

namespace IronText.Runtime
{
    sealed class TaggedStack<T> 
        : IStackLookback<T>
        , ITaggedStack<T>
        , IUndoable
    {
        private int[]   tags;
        private T[]     data;
        private int     Capacity;
        private int      ValueCount;
        private int      TagCount;

        // TODO: Get rid of this because it overcomplicates stack access and causes perf issues.
        //       Planning to remove it after introducing ECLR semantics.
        public int currentRecoveryStart;
        public int currentRecoveryCount;
        private readonly CircularStack<int> savedTags = new CircularStack<int>(100);
        private readonly CircularStack<T> savedData = new CircularStack<T>(100);
        private readonly CircularStack<RecoveryInterval> recoveryIntervals = new CircularStack<RecoveryInterval>(1);

        public TaggedStack(int capacity)
        {
            this.tags = new int[capacity];
            this.data = new T[capacity];
            this.Capacity = capacity;
        }

        private TaggedStack(int[] tags, int count)
        {
            this.tags = tags;
            this.data = new T[tags.Length];
            this.Capacity = tags.Length;
            this.TagCount = this.ValueCount = count;
        }

        public int Count
        {
            get
            {
                if (ValueCount != TagCount)
                {
                    throw new InvalidOperationException();
                }

                return ValueCount;
            }
        }

        public T Peek() { return data[ValueCount - 1]; }

        public int PeekTag() { return tags[TagCount - 1]; }

        public void Push(int tag, T msg)
        {
            PushValue(msg);
            PushTag(tag);
        }

        public void PushTag(int tag)
        {
            tags[TagCount++] = tag;
        }

        public void PushValue(T msg)
        {
            if (ValueCount == Capacity)
            {
                Grow();
            }

            data[ValueCount++] = msg;
        }

        public void PopTag()
        {
            --TagCount;
        }

        public void Pop(int count)
        {
            int newCount = Count - count;

            while (currentRecoveryStart > newCount)
            {
                --currentRecoveryStart;
                ++currentRecoveryCount;
                savedTags.Push(tags[currentRecoveryStart]);
                savedData.Push(data[currentRecoveryStart]);
            }

            InternalPop(count);
        }

        private void RequireStacksSynchronized()
        {
            if (TagCount != ValueCount)
            {
                throw new InvalidOperationException();
            }
        }

        private void InternalPop(int count)
        {
            RequireStacksSynchronized();

            ValueCount -= count;
            TagCount = ValueCount;

            switch (count)
            {
                case 0:
                    break;
                case 1:
                    data[ValueCount]     = default(T);
                    break;
                case 2:
                    data[ValueCount]     = default(T);
                    data[ValueCount + 1] = default(T);
                    break;
                case 3:
                    data[ValueCount]     = default(T);
                    data[ValueCount + 1] = default(T);
                    data[ValueCount + 2] = default(T);
                    break;
                default:
                    Array.Clear(data, ValueCount, count);
                    break;
            }
        }

        public void Clear()
        {
            Array.Clear(data, 0, ValueCount);
            ValueCount = 0;
            TagCount = 0;
        }

        private void Grow()
        {
            int newSize = Capacity * 2;
            Array.Resize(ref data, newSize);
            Array.Resize(ref tags, newSize);
            Capacity = newSize;
        }

        public int GetState(int backoffset)
        {
            if (backoffset <= 0)
            {
                throw new ArgumentException(nameof(backoffset));
            }

            return tags[TagCount - backoffset];
        }

        public T GetNodeAt(int backoffset)
        {
            if (backoffset <= 0)
            {
                throw new ArgumentException(nameof(backoffset));
            }

            return data[ValueCount - backoffset];
        }

        public void BeginEdit()
        {
            RequireStacksSynchronized();

            currentRecoveryStart = ValueCount;
            currentRecoveryCount = 0;
        }

        public void EndEdit()
        {
            recoveryIntervals.Push(
                new RecoveryInterval
                {
                    Start = currentRecoveryStart,
                    Count = currentRecoveryCount,
                });
        }

        public void Undo(int inputCount)
        {
            if (inputCount > 1)
            {
                throw new ArgumentOutOfRangeException("Incorrect recovery point count.");
            }

            RequireStacksSynchronized();

            // Cancel current input save
            InternalPop(ValueCount - currentRecoveryStart);
            int i = currentRecoveryCount;
            while (i-- != 0)
            {
                Push(savedTags.Pop(), savedData.Pop());
            }

            // Undo previous inputCount inputs
            while (inputCount-- != 0)
            {
                var interval = recoveryIntervals.Pop();
                InternalPop(ValueCount - interval.Start);
                i = interval.Count;
                while (i-- != 0)
                {
                    Push(savedTags.Pop(), savedData.Pop());
                }
            }
        }

        struct RecoveryInterval
        {
            public int Start;
            public int Count;
        }

        public TaggedStack<object> CloneWithoutData()
        {
            return new TaggedStack<object>((int[])tags.Clone(), TagCount);
        }
    }
}
