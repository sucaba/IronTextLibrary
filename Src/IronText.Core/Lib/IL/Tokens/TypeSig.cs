using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace IronText.Lib.IL
{
    public class TypeSig
    {
        private const string typePattern = @"
            (?: \[  (?<assembly>[a-zA-Z_.0-9]+)  \] )?
            (?: (?<namespace>[a-zA-Z_][a-zA-Z_.0-9]*)  [.])?
            (?<typeName>[a-zA-Z_][a-zA-Z_/0-9]*)
            (?<refSuffix> [&] )?
            (?<arraySuffix> \[\] )?
            ";

        private static readonly Regex typeRegex = new Regex(typePattern, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

        public string Assembly;
        public string Namespace;
        public string[] TypeNameParts;
        public TypeSig[] TypeParameters;
        public bool IsArray;
        public bool IsValue;
        public bool IsByRef;

        public static TypeSig FromName(string fullName)
        {
            var result = new TypeSig();

            int typeEnd = fullName.IndexOf(',');
            if (typeEnd >= 0)
            {
                fullName = fullName.Substring(0, typeEnd);
                result.Assembly = fullName.Substring(typeEnd + 1);
            }

            if (fullName.EndsWith("[]"))
            {
                result.IsArray = true;
                fullName = fullName.Substring(0, fullName.Length - 2);
            }

            int namespaceEnd = fullName.LastIndexOf('.');
            if (namespaceEnd >= 0)
            {
                result.Namespace = fullName.Substring(0, namespaceEnd);
                result.TypeNameParts = new string[] { fullName.Substring(namespaceEnd + 1) };
            }
            else
            {
                result.Namespace = "";
                result.TypeNameParts = new string[] { fullName };
            }

            return result;
        }

        public static TypeSig FromType(Type type)
        {
            if (type.IsByRef)
            {
                var result = FromType(type.GetElementType());
                result.IsByRef = true;
                return result;
            }

            if (type.IsArray)
            {
                var result = FromType(type.GetElementType());
                result.IsArray = true;
                return result;
            }

            var parts = new List<string>();
            Type partType = type;
            Type topmost;

            do
            {
                topmost = type;
                parts.Add(partType.Name);
                partType = partType.DeclaringType;
            }
            while (partType != null);

            parts.Reverse();

            return new TypeSig
            {
                Assembly      = topmost.Assembly.FullName,
                Namespace     = topmost.Namespace,
                TypeNameParts = parts.ToArray(),
                TypeParameters = type.IsGenericType ? Array.ConvertAll(type.GetGenericArguments(), FromType) : new TypeSig[0],
                IsValue       = type.IsValueType
            };
        }

        public static TypeSig Parse(string text)
        {
            text = ResolveAlias(text);

            var match = typeRegex.Match(text);

            if (!match.Success)
            {
                throw new InvalidOperationException(
                    string.Format("Invalid type signature: '{0}'", text));
            }

            var result = new TypeSig
            {
                Assembly        = match.Groups["assembly"].Value,
                Namespace       = match.Groups["namespace"].Value,
                TypeNameParts   = match.Groups["typeName"].Value.Split('/'),
                IsArray         = match.Groups["arraySuffix"].Success,
                IsByRef         = match.Groups["refSuffix"].Success
            };

            return result;
        }

        public string TypeName
        {
            get
            {
                var output = new StringBuilder();
                output.Append(Namespace).Append('.');
                foreach (var part in TypeNameParts)
                {
                    output.Append(part);
                }

                if (TypeParameters.Length != 0)
                {
                    output.Append("<");
                    bool first = true;
                    foreach (var typeSign in TypeParameters)
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            output.Append(",");
                        }

                        output.Append(typeSign.TypeName);
                    }

                    output.Append(">");
                }

                if (IsArray)
                {
                    output.Append("[]");
                }

                return output.ToString();
            }
        }

        public Type LoadType() { return Type.GetType(TypeName); }

        private static string ResolveAlias(string typeSignature)
        {
            string result;
            switch (typeSignature)
            {
                case "void":        result = "[mscorlib]System.Void"; break;
                case "object":      result = "[mscorlib]System.Object"; break;
                case "string":      result = "[mscorlib]System.String"; break;
                case "int32":       result = "[mscorlib]System.Int32"; break;
                case "native int":  result = "[mscorlib]System.Int32"; break;
                default:            result = typeSignature; break;
            }

            return result;
        }

        public static void SplitFullName (string fullname, out string @namespace, out string name)
        {
            var last_dot = fullname.LastIndexOf ('.');

            if (last_dot == -1) {
                @namespace = string.Empty;
                name = fullname;
            } else {
                @namespace = fullname.Substring (0, last_dot);
                name = fullname.Substring (last_dot + 1);
            }
        }
    }

}
