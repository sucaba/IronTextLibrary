using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IronText.Algorithm;
using IronText.Extensibility;
using IronText.Extensibility.Cil;
using IronText.Framework;
using IronText.Framework.Reflection;
using IronText.Misc;

namespace IronText.MetadataCompiler
{
    public static class CilProviderExtensions
    {
        public static IEnumerable<MethodInfo> GetGetterPath(this CilContextProvider provider, Type type)
        {
            return new ContextBrowser(provider.ProviderType).GetGetterPath(type);
        }

        public static IEnumerable<Type> GetAllContextTypes(this CilContextProvider provider)
        {
            return new ContextBrowser(provider.ProviderType).GetAllContextTypes();
        }
    }

    class ContextBrowser
    {
        private readonly Type fromType;

        public ContextBrowser(Type fromType)
        {
            this.fromType = fromType;
        }

        public IEnumerable<MethodInfo> GetGetterPath(Type type)
        {
            if (type.IsAssignableFrom(fromType))
            {
                return Enumerable.Empty<MethodInfo>();
            }

            var path = Graph.Search(
                        EnumerateContextGetters(fromType),
                        m => EnumerateContextGetters(m.ReturnType),
                        m => type.IsAssignableFrom(m.ReturnType));

            return path;
        }

        public IEnumerable<Type> GetAllContextTypes()
        {
            var result = Graph.AllVertexes(
                        EnumerateContextGetters(fromType),
                        m => EnumerateContextGetters(m.ReturnType))
                        .Select(m => m.ReturnType)
                        .ToList();

            return result;
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
