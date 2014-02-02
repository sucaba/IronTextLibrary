using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using SR = System.Reflection;

namespace IronText.Misc
{
    internal static class ReflectionUtils
    {
        public static string DescribeValue(object instance)
        {
            const string indent = "  ";

            var output = new StringBuilder();
            var type = instance.GetType();
            output.AppendLine(type.ToString());

            foreach (var field in type.GetFields(SR.BindingFlags.Instance | SR.BindingFlags.NonPublic | SR.BindingFlags.Public))
            {
                output
                    .Append(indent)
                    .Append(field.Name)
                    .Append(" = ")
                    .Append(field.GetValue(instance))
                    .AppendLine();
            }
            output.AppendLine().AppendLine();
            return output.ToString();
        }
    

        public static IEnumerable<MethodInfo> EnumeratePublishedMethods(Type type)
        {
            var result = new List<MethodInfo>();

            result.AddRange(type.GetMethods());

            foreach (var @interface in type.GetInterfaces())
            {
                result.AddRange(EnumeratePublishedMethods(@interface));
            }

            return result;
        }

        public static IEnumerable<Type> EnumerateContextTypes(Type type)
        {
            var result = new List<Type>();

            result.Add(type);

            foreach (var @interface in type.GetInterfaces())
            {
                result.AddRange(EnumerateContextTypes(@interface));
            }

            return result;
        }

        public static void GetDelegateSignature(Type delegateType, out Type resultType, out Type[] argTypes)
        {
            var method = delegateType.GetMethod("Invoke");
            argTypes = method.GetParameters().Select(param => param.ParameterType).ToArray();
            resultType = method.ReturnType;
        }

        public static string ToString(MemberInfo memberInfo)
        {
            var output = new StringBuilder();
            var asType = memberInfo as Type;
            if (asType != null)
            {
                output.Append(asType.FullName);
            }
            else
            {
                var type = memberInfo.DeclaringType;
                output.Append(type.FullName).Append("::").Append(memberInfo.Name);
            }

            return output.ToString();
        }
    }
}
