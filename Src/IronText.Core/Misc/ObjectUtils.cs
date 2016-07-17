namespace IronText.Misc
{
    static class ObjectUtils
    {
        public static void Swap<T>(ref T x, ref T y)
        {
            T temp = x;
            x = y;
            y = temp;
        }

        public static void RotateLeft<T>(ref T x, ref T y, ref T z)
        {
            T temp = x;
            x = y;
            y = z;
            z = temp;
        }

        public static void RotateRight<T>(ref T x, ref T y, ref T z) =>
            RotateLeft(ref z, ref y, ref x);
    }
}
