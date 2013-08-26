using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IronText.Algorithm;
using IronText.Framework;
using IronText.Misc;

namespace IronText.MetadataCompiler
{
    class ContextBrowser
    {
        private readonly Type rootContextType;

        public ContextBrowser(Type rootContextType)
        {
            this.rootContextType = rootContextType;
        }

        public IEnumerable<MethodInfo> GetGetterPath(Type type)
        {
            if (type.IsAssignableFrom(rootContextType))
            {
                return Enumerable.Empty<MethodInfo>();
            }

            var path = Graph.Search(
                        EnumerateContextGetters(rootContextType),
                        m => EnumerateContextGetters(m.ReturnType),
                        m => type.IsAssignableFrom(m.ReturnType));

            return path;
        }

        private static IEnumerable<MethodInfo> EnumerateContextGetters(Type type)
        {
            return 
                from prop in EnumeratePublicInstanceProperties(type)
                where Attributes.Exists<SubContextAttribute>(prop)
                select prop.GetGetMethod();
        }

        private static IEnumerable<PropertyInfo> EnumeratePublicInstanceProperties(Type type)
        {
            var result = new List<PropertyInfo>();
            result.AddRange(type.GetProperties(BindingFlags.Public | BindingFlags.Instance));
            if (type.IsInterface)
            {
                result.AddRange(type.GetInterfaces().SelectMany(EnumeratePublicInstanceProperties));
            }

            return result;
        }
    }
}
