using System;

namespace IronText.Framework
{
    public class FinalReceiver<T> : IReceiver<T>
    {
        public static readonly IReceiver<T> Instance = new FinalReceiver<T>();

        public IReceiver<T> Next(T item)
        {
            throw new InvalidOperationException("Receiver cannot receive data.");
        }

        public IReceiver<T> Done()
        {
            throw new InvalidOperationException("Receiver cannot receive data.");
        }
    }
}
