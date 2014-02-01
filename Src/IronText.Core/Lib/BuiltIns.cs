using System.Collections.Generic;
using IronText.Framework;

namespace IronText.Lib
{
    [Vocabulary]
    public static class Builtins
    {
        [Produce]
        public static List<T> List<T>() { return new List<T>(4); }

        [Produce]
        public static List<T> List<T>(List<T> items, T item) { items.Add(item); return items; }

        [Produce]
        public static T[] Array<T>(T x) { return new T[] { x }; }

        [Produce]
        public static T[] Array<T>(T x, T y) { return new T[] { x, y }; }

        [Produce]
        public static T[] Array<T>(T x, T y, T z) { return new T[] { x, y, z }; }

        [Produce]
        public static T[] Array<T>(T x, T y, T z, T t, List<T> more) 
        {
            T[] result = new T[4 + more.Count];
            result[0] = x;
            result[1] = y;
            result[2] = z;
            result[3] = t;
            more.CopyTo(result, 4);
            return result;
        }
    }
}
