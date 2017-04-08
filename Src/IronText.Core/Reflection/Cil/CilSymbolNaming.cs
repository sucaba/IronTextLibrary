using System;
using System.Text;

namespace IronText.Reflection.Managed
{
    static class CilSymbolNaming
    {
        public const string StartTokenName = "void";
        public const string StringTokenName = "$id";
        public const string BoolTokenType = "bool";

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

            if (typeof(bool) == type)
            {
                return BoolTokenType;
            }

            if (type.IsArray)
            {
                return GetTypeName(type.GetElementType()) + "[]";
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
