using System.Collections.Generic;

namespace IronText.Runtime
{
    public interface IReceiver<T>
    {
        IReceiver<T>    Next(T item);

        IReceiver<T>    Done();
    }

    public static class Reciever
    {
        public static IReceiver<T> Feed<T>(this IReceiver<T> initial, params T[] items)
        {
            IReceiver<T> current = initial;

            foreach (var item in items)
            {
                if (current == null)
                {
                    return null;
                }

                current = current.Next(item);
            }

            return current;
        }

        public static IReceiver<T> Feed<T>(this IReceiver<T> initial, IEnumerable<T> items)
        {
            IReceiver<T> current = initial;

            foreach (var item in items)
            {
                if (current == null)
                {
                    return null;
                }

                current = current.Next(item);
            }

            return current;
        }
    }
}
