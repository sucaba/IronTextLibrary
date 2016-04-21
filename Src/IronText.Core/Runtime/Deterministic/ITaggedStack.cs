using IronText.Algorithm;

namespace IronText.Runtime
{
    interface ITaggedStack<T>
    {
        int PeekTag();

        T Peek();

        void Pop(int count);

        void Push(int tag, T value);

        void Clear();

        TaggedStack<object> CloneWithoutData();
    }
}
