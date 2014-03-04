﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using IronText.Algorithm;
using IronText.Misc;
using IronText.Framework;

namespace IronText.Reflection.Managed
{
    public class CilContextProvider
    {
        public CilContextProvider(Type providerType)
        {
            this.ProviderType     = providerType;
        }

        public Type ProviderType     { get; private set; }

        public IEnumerable<MethodInfo> GetGetterPath(Type type)
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

        public IEnumerable<Type> GetAllContextTypes()
        {
            var result = Graph.AllVertexes(
                        EnumerateContextGetters(ProviderType),
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
