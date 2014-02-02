
namespace IronText.Collections
{
    public interface IOwner<T>
    {
        void Acquire(T item);

        void Release(T item);
    }
}
