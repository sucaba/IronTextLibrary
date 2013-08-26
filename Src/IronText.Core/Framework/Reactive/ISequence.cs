
namespace IronText.Framework
{
    public interface ISequence<T>
    {
        IReceiver<T> Accept(IReceiver<T> visitor);
    }
}
