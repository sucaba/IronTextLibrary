using IronText.Algorithm;

namespace IronText.Framework
{
    interface ITaggedStack<T>
    {
        int PeekTag();

        T Peek();

        ArraySlice<T> PeekTail(int size);

        void Pop(int count);

        void Push(int tag, T value);

        void Clear();

        TaggedStack<object> CloneWithoutData();
    }
}
