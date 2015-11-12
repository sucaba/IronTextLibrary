using IronText.Algorithm;
using IronText.Framework;
using IronText.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IronText.Reflection.Managed
{
    public class CilSemanticScope : IEnumerable<KeyValuePair<CilSemanticRef,CilSemanticValue>>
    {
        private readonly Dictionary<CilSemanticRef, CilSemanticValue> dictionary;
        private readonly Type providerType;

        public CilSemanticScope(Type providerType)
        {
            this.providerType = providerType;
            this.dictionary   = BuildDictionary();
        }

        public CilSemanticValue Resolve(CilSemanticRef name)
        {
            return dictionary[name];
        }

        IEnumerator<KeyValuePair<CilSemanticRef, CilSemanticValue>> IEnumerable<KeyValuePair<CilSemanticRef, CilSemanticValue>>.GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        private Dictionary<CilSemanticRef,CilSemanticValue> BuildDictionary()
        {
            var result = new Dictionary<CilSemanticRef,CilSemanticValue>();

            result.Add(
                CilSemanticRef.ByType(providerType),
                new CilSemanticValue(providerType, new MethodInfo[0]));

            var types =
                Graph.BreadthFirst(
                        EnumerateContextGetters(providerType),
                        m => EnumerateContextGetters(m.ReturnType))
                        .Select(m => m.ReturnType)
                        .Distinct()
                        ;

            foreach (var type in types)
            {
                result.Add(
                    CilSemanticRef.ByType(type),
                    new CilSemanticValue(type, GetGetterPath(type)));
            }

            return result;
        }

        private static KeyValuePair<T,V> Pair<T,V>(T key, V value)
        {
            return new KeyValuePair<T, V>(key, value);
        }

        private IEnumerable<MethodInfo> GetGetterPath(Type type)
        {
            if (type.IsAssignableFrom(providerType))
            {
                return Enumerable.Empty<MethodInfo>();
            }

            var path = Graph.BreadthFirstSearch(
                        EnumerateContextGetters(providerType),
                        m => EnumerateContextGetters(m.ReturnType),
                        m => type.IsAssignableFrom(m.ReturnType));

            if (path == null)
            {
                throw new InvalidOperationException("Internal error");
            }

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
            result.AddRange(type.GetProperties(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance));
            if (type.IsInterface)
            {
                result.AddRange(type.GetInterfaces().SelectMany(EnumeratePublicInstanceProperties));
            }

            return result;
        }
    }
}
