using IronText.Collections;
using System;

namespace IronText.Runtime
{
    class GenericStack<T>
        : IUndoable
    {
        private MutableArray<ParserThread<T>> front;
        private MutableArray<ParserThread<T>> toRemove;

        public GenericStack(int capacity)
        {
            front    = new MutableArray<ParserThread<T>>(capacity);
            toRemove = new MutableArray<ParserThread<T>>(capacity);
        }

        public ImmutableArray<ParserThread<T>> Front => front;

        public void AddThread(int state, T value)
        {
            front.Add(new ParserThread<T>(state, value));
        }

        public void Clear()
        {
            front.Clear();
        }

        public ParserThread<T> Push(
            ParserThread<T> previous,
            int             state,
            T               value)
        {
            var result = new ParserThread<T>(state, value, previous);
            front.Replace(previous, result);
            return result;
        }

        public bool IsAlive(ParserThread<T> thread)
        {
            return !toRemove.Contains(thread);
        }

        public void Remove(ParserThread<T> thread)
        {
            toRemove.Add(thread);
        }

        public ParserThread<T> Pop(ParserThread<T> thread, int count)
        {
            var result = thread;
            while (count-- != 0)
            {
                result = result.Previous;
            }

            front.Replace(thread, result);
            return result;
        }

        public void PopLayer()
        {
            throw new NotImplementedException();
        }

        public void BeginEdit()
        {
        }

        public void EndEdit()
        {
            foreach (var thread in toRemove)
            {
                front.Remove(thread);
            }

            toRemove.Clear();
        }

        public GenericStack<object> CloneWithoutData()
        {
            throw new NotImplementedException();
        }

        public int HasLayers { get { throw new NotImplementedException(); } }

        public bool IsEmpty => front.Count <= toRemove.Count;

        public void Undo(int undoCount)
        {
            throw new NotImplementedException();
        }

        public ParserThread<T> Fork(ParserThread<T> thread)
        {
            var result = thread.Clone();
            front.Add(result);
            return result;
        }
    }
}
