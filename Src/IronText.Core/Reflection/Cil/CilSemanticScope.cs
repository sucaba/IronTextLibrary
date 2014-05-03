using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using IronText.Algorithm;
using IronText.Misc;
using IronText.Framework;

namespace IronText.Reflection.Managed
{
    public class CilSemanticScope
    {
        private Dictionary<string, CilSemanticValue> dictionary;

        public CilSemanticScope(Type providerType)
        {
            this.ProviderType     = providerType;
        }

        public Type ProviderType     { get; private set; }

        public CilSemanticValue Resolve(string name)
        {
            return Contexts.Single(c => c.Key == name).Value;
        }

        public IEnumerable<KeyValuePair<string,CilSemanticValue>> Contexts
        {
            get
            {
                var result = new List<CilSemanticValue>
                { 
                    new CilSemanticValue(ProviderType, new MethodInfo[0]) 
                };

                result.AddRange(
                    Graph.AllVertexes(
                            EnumerateContextGetters(ProviderType),
                            m => EnumerateContextGetters(m.ReturnType))
                            .Distinct()
                            .Select(m => new CilSemanticValue(m.ReturnType, GetGetterPath(m.ReturnType))));

                return result.Select(c => new KeyValuePair<string,CilSemanticValue>(c.UniqueName, c));
            }
        }

        private IEnumerable<MethodInfo> GetGetterPath(Type type)
        {
            if (type.IsAssignableFrom(ProviderType))
            {
                return Enumerable.Empty<MethodInfo>();
            }

            var path = Graph.Search(
                        EnumerateContextGetters(ProviderType),
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
