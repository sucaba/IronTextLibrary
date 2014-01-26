using System.Collections.Generic;
using IronText.Framework;
using IronText.Runtime;

namespace IronText.Tests.TestUtils
{
    /// <summary>
    /// TODO: Replace with QueueReciever
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class Collector<T> : List<T>, IReceiver<T>
    {
        public IReceiver<T> Next(T item)
        {
            this.Add(item);
            return this;
        }

        public IReceiver<T> Done()
        {
            return this;
        }
    }
}
