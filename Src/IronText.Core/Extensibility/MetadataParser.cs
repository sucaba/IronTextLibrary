using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IronText.Misc;

namespace IronText.Extensibility
{
    static class MetadataParser
    {
        public static IEnumerable<ILanguageMetadata> EnumerateAndBind(MemberInfo member)
        {
            return EnumerateAndBind(null, member);
        }

        public static IEnumerable<ILanguageMetadata> EnumerateAndBind(ILanguageMetadata parent, MemberInfo member)
        {
            var result = Attributes.All<ILanguageMetadata>(member);
            foreach (var m in result)
            {
                m.Bind(parent, member);
            }

            return result;
        }

        public static IEnumerable<ILanguageMetadata> GetTypeMetaChildren(ILanguageMetadata parent, Type type)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

            var result = new List<ILanguageMetadata>();
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

        private static void GetMemberMetadata(ILanguageMetadata parent, List<ILanguageMetadata> result, MemberInfo member)
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
