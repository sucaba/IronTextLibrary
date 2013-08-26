using System;
using System.Text.RegularExpressions;

namespace IronText.Lib.IL
{
    public class FieldSig
    {
        private const string pattern = 
            @"(?<classType>\S+)" +
            @"\s* :: \s*" + 
            @"(?<fieldName>[a-zA-Z_][a-zA-Z_0-9]*)"
            ;

        private static readonly Regex regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

        public string Name;
        public TypeSig DeclaringTypeSig;

        public static FieldSig Parse(string fieldSignature)
        {
            Match match = regex.Match(fieldSignature);
            if (!match.Success)
            {
                throw new InvalidOperationException(
                    string.Format("Invalid method signature: '{0}'", fieldSignature));
            }

            var result = new FieldSig
            {
                Name = match.Groups["fieldName"].Value,
                DeclaringTypeSig = TypeSig.Parse(match.Groups["classType"].Value)
            };

            return result;
        }
    }
}
