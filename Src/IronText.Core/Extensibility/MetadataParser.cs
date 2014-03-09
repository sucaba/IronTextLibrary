using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IronText.Misc;

namespace IronText.Extensibility
{
    static class MetadataParser
    {
        public static IEnumerable<ICilMetadata> EnumerateAndBind(MemberInfo member)
        {
            return EnumerateAndBind(null, member);
        }

        public static IEnumerable<ICilMetadata> EnumerateAndBind(ICilMetadata parent, MemberInfo member)
        {
            var result = Attributes.All<ICilMetadata>(member);
            foreach (var m in result)
            {
                m.Bind(parent, member);
            }

            return result;
        }

        public static IEnumerable<ICilMetadata> GetTypeMetaChildren(ICilMetadata parent, Type type)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Instance;

            var result = new List<ICilMetadata>();
            foreach (var member in type.GetMembers(flags))
            {
                GetMemberMetadata(parent, result, member);
            }

            if (type.BaseType != null)
            {
                result.AddRange(EnumerateAndBind(parent, type.BaseType));
            }

            result.AddRange(
                type
                    .GetInterfaces()
                    .SelectMany(intf => MetadataParser.EnumerateAndBind(parent, intf)));

            return result;
        }

        private static void GetMemberMetadata(ICilMetadata parent, List<ICilMetadata> result, MemberInfo member)
        {
            foreach (var attr in MetadataParser.EnumerateAndBind(parent, member))
            {
                result.Add(attr);
            }

            var method = member as MethodInfo;
            if (method != null)
            {
                foreach (var attr in MetadataParser.EnumerateAndBind(method.ReturnType))
                {
                    result.Add(attr);
                }

                foreach (var param in method.GetParameters())
                    foreach (var attr in MetadataParser.EnumerateAndBind(param.ParameterType))
                    {
                        result.Add(attr);
                    }
            }
        }
    }
}
