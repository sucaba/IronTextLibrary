
namespace IronText.Runtime
{
    public interface ISequence<T>
    {
        IReceiver<T> Accept(IReceiver<T> visitor);
    }
}
