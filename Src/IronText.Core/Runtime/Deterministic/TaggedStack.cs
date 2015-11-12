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
        public int      Count;
        public int      Start;
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
            this.Count = count;
        }

        public T Peek() { return data[Count - 1]; }

        public int PeekTag() { return tags[Count - 1]; }

        public void Push(int tag, T msg)
        {
            if (Count == Capacity)
            {
                Grow();
            }

            tags[Count] = tag;
            data[Count] = msg;
            ++Count;
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

        private void InternalPop(int count)
        {
            Count -= count;
            switch (count)
            {
                case 0:
                    break;
                case 1:
                    data[Count]     = default(T);
                    break;
                case 2:
                    data[Count]     = default(T);
                    data[Count + 1] = default(T);
                    break;
                case 3:
                    data[Count]     = default(T);
                    data[Count + 1] = default(T);
                    data[Count + 2] = default(T);
                    break;
                default:
                    Array.Clear(data, Count, count);
                    break;
            }
        }

        public void Clear()
        {
            Array.Clear(data, 0, Count);
            Count = 0;
        }

        private void Grow()
        {
            int newSize = Capacity * 2;
            Array.Resize(ref data, newSize);
            Array.Resize(ref tags, newSize);
            Capacity = newSize;
        }

        public ArraySlice<T> PeekTail(int size)
        {
            return new ArraySlice<T>(data, Count - size, size);
        }

        public int GetParentState() { return tags[Start - 1]; }

        public T GetNodeAt(int backOffset)
        {
            return data[Start - backOffset];
        }

        public void BeginEdit()
        {
            currentRecoveryStart = Count;
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

            // Cancel current input save
            InternalPop(Count - currentRecoveryStart);
            int i = currentRecoveryCount;
            while (i-- != 0)
            {
                Push(savedTags.Pop(), savedData.Pop());
            }

            // Undo previous inputCount inputs
            while (inputCount-- != 0)
            {
                var interval = recoveryIntervals.Pop();
                InternalPop(Count - interval.Start);
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
            return new TaggedStack<object>((int[])tags.Clone(), Count);
        }
    }
}
