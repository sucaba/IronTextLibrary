using IronText.Framework;

namespace IronText.Lib.NfaVM
{
    public class Zom_<T> { }

    [Vocabulary]
    public static class Primitives
    {
        [Produce]
        public static Zom_<T> Zom_<T>() { return null; }

        [Produce]
        public static Zom_<T> Zom_<T>(Zom_<T> items, T item) { return null; }

        public static Zom_<T> Zom_<T>(params T[] items)
        {
            Zom_<T> result = Zom_<T>();
            foreach (var item in items)
            {
                result = Zom_(result, item);
            }

            return null; 
        }
    }
}
