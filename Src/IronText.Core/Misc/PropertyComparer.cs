using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IronText.Misc
{
    internal class PropertyComparer<T> : IEqualityComparer<T> where T : class
    {
        public static readonly IEqualityComparer<T> Default = new PropertyComparer<T>();

        public const BindingFlags DefaultBindingFlags 
            = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
        
        private readonly BindingFlags flags;

        public PropertyComparer() : this(DefaultBindingFlags) { }

        public PropertyComparer(BindingFlags flags)
        {
            this.flags = flags;
        }

        public bool Equals(T x, T y)
        {
            if ((object)x == (object)y)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            var thisType = x.GetType();
            var otherType = y.GetType();
            if (thisType != otherType)
            {
                return false;
            }

            var properties = thisType.GetProperties(flags);
            foreach (var prop in properties)
            {
                if (prop.GetIndexParameters().Length != 0)
                {
                    // Skip indexed properties
                    continue;
                }

                var getter = prop.GetGetMethod();
                if (getter == null || (getter.Attributes & MethodAttributes.Public) != MethodAttributes.Public)
                {
                    // Skip protected and properties without getter
                    continue;
                }

                object xValue = prop.GetValue(x, null);
                object yValue = prop.GetValue(y, null);
                if (!object.Equals(xValue, yValue))
                {
                    var xAsEnumerable = xValue as IEnumerable<object>;
                    var yAsEnumerable = yValue as IEnumerable<object>;
                    if (xAsEnumerable == null 
                        || yAsEnumerable == null 
                        || !Enumerable.SequenceEqual(xAsEnumerable, yAsEnumerable))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public int GetHashCode(T obj)
        {
            if (obj == null)
            {
                return 0;
            }

            int result = 0;

            var type = obj.GetType();
            var properties = type.GetProperties(flags);
            foreach (var prop in properties)
            {
                object propValue = prop.GetValue(obj, null);
                if (propValue != null)
                {
                    unchecked
                    {
                        result += propValue.GetHashCode();
                    }
                }
            }

            return result;
        }
    }
}
