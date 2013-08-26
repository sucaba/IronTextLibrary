using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace IronText.Lib.IL
{
    public class MethodSig
    {
        private const string methodPattern = 
            @"(?<returnType>\S+)" +
            @"\s+" +
            @"(?<classType>\S+)" +
            @"\s* :: \s*" + 
            @"(?<methodName>[a-zA-Z_][a-zA-Z_0-9]*)" +
            @"\s*[(]\s* (?<args>[^)]*)  \s*[)]"
            ;

        private static readonly Regex methodRegex = new Regex(methodPattern, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

        public TypeSig ReturnTypeSig;
        public string Name;
        public IEnumerable<ParamSig> Args;
        public TypeSig DeclaringTypeSig;

        public MethodSig()
        {
        }

        public static MethodSig Parse(string methodSignature)
        {
            Match match = methodRegex.Match(methodSignature);
            if (!match.Success)
            {
                throw new InvalidOperationException(
                    string.Format("Invalid method signature: '{0}'", methodSignature));
            }

            var methodSig = new MethodSig
            {
                ReturnTypeSig    = TypeSig.Parse(match.Groups["returnType"].Value),
                Name             = match.Groups["methodName"].Value,
                Args             = Regex.Split(match.Groups["args"].Value, @"\s*,\s*").Select(ParamSig.Parse),
                DeclaringTypeSig = TypeSig.Parse(match.Groups["classType"].Value)
            };

            return methodSig;
        }

        public static MethodSig GetDelegateMethodSignature(TypeSig declaringType, string name, Type delegateType)
        {
            var method = delegateType.GetMethod("Invoke");
            var parameters = method.GetParameters();
            return new MethodSig
            {
                Name             = name,
                ReturnTypeSig    = TypeSig.FromType(method.ReturnType),
                DeclaringTypeSig = declaringType,
                Args             = parameters.Select(ParamSig.FromParameterInfo).ToArray()
            };
        }
    }
}
