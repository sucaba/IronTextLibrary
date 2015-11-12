
namespace IronText.Misc
{
    public interface IDiscriminatable
    {
    }

    public static class Discriminatable
    {
        public static bool Match<T,T1>(this T self, out T1 val1) 
            where T  : class, IDiscriminatable
            where T1 : class, T
        {
            val1 = self as T1;
            return val1 != null;
        }

        public static int Match<T,T1,T2>(this T self, out T1 val1, out T2 val2) 
            where T  : class, IDiscriminatable
            where T1 : class, T 
            where T2 : class, T
        {
            val1 = self as T1;
            if (val1 != null)
            {
                val2 = null;
                return 0;
            }

            val2 = self as T2;
            if (val2 != null)
            {
                return 1;
            }
            
            return -1;
        }

        public static int Match<T,T1,T2,T3>(this T self, out T1 val1, out T2 val2, out T3 val3) 
            where T  : class, IDiscriminatable
            where T1 : class, T 
            where T2 : class, T
            where T3 : class, T
        {
            val1 = self as T1;
            if (val1 != null)
            {
                val2 = null;
                val3 = null;
                return 0;
            }

            val2 = self as T2;
            if (val2 != null)
            {
                val3 = null;
                return 1;
            }

            val3 = self as T3;
            if (val3 != null)
            {
                return 2;
            }
            
            return -1;
        }
    }
}
