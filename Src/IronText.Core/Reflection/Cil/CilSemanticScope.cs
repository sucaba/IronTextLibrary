using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using IronText.Algorithm;
using IronText.Misc;
using IronText.Framework;

namespace IronText.Reflection.Managed
{
    public class CilSemanticScope : IEnumerable<KeyValuePair<string,CilSemanticValue>>
    {
        private readonly Dictionary<string, CilSemanticValue> dictionary;
        private readonly Type providerType;

        public CilSemanticScope(Type providerType)
        {
            this.providerType = providerType;
            this.dictionary   = BuildDictionary();
        }

        public CilSemanticValue Resolve(string name)
        {
            return dictionary[name];
        }

        IEnumerator<KeyValuePair<string, CilSemanticValue>> IEnumerable<KeyValuePair<string, CilSemanticValue>>.GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        private Dictionary<string,CilSemanticValue> BuildDictionary()
        {
            var result = new Dictionary<string,CilSemanticValue>();

            result.Add(
                CilSemanticRef.ByType(providerType).UniqueName,
                new CilSemanticValue(providerType, new MethodInfo[0]));

            var types =
                Graph.AllVertexes(
                        EnumerateContextGetters(providerType),
                        m => EnumerateContextGetters(m.ReturnType))
                        .Select(m => m.ReturnType)
                        .Distinct()
                        ;

            foreach (var type in types)
            {
                result.Add(
                    CilSemanticRef.ByType(type).UniqueName,
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

            var path = Graph.Search(
                        EnumerateContextGetters(providerType),
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
            result.AddRange(type.GetProperties(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance));
            if (type.IsInterface)
            {
                result.AddRange(type.GetInterfaces().SelectMany(EnumeratePublicInstanceProperties));
            }

            return result;
        }
    }
}
