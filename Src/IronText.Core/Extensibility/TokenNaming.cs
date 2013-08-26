using System;
using System.Text;

namespace IronText.Extensibility
{
    static class TokenNaming
    {
        public const string StartTokenName = "void";
        public const string StringTokenName = "$id";

        public static string GetTypeName(Type type)
        {
            if (typeof(void) == type)
            {
                return StartTokenName;
            }

            if (typeof(string) == type)
            {
                return StringTokenName;
            }

            if (!type.IsGenericType)
            {
                return type.Name;
            }

            var result = new StringBuilder();
            var name = type.GetGenericTypeDefinition().Name;
            name = name.Substring(0, name.IndexOf('`'));
            result.Append(name).Append('<');
            bool first = true;
            foreach (var paramType in type.GetGenericArguments())
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    result.Append(", ");
                }

                result.Append(GetTypeName(paramType));
            }

            result.Append('>');
            return result.ToString();
        }

        public static string GetLiteralName(string literal)
        {
            return "'" + literal + "'";
        }
    }
}
