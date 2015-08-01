using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace IronText.Reflection
{
    internal class DotExpression
    {
        private static Regex regex = new Regex(@"\s* ([^!.\s]+) \s* ([!.?]) \s* ([^!.\s]+) \s*", RegexOptions.Compiled|RegexOptions.IgnorePatternWhitespace);

        public static string[] Parse(string expr)
        {
            if (expr == null)
            {
                throw new ArgumentNullException("dotExpression");
            }

            var result =  new string[3];
            var m = regex.Match(expr);
            if (!m.Success)
            {
                var msg = string.Format("Invalid dot expression '{0}'. Expected format is '<symbol-name>[.?]<property>'.", expr);
                throw new ArgumentException(msg, "dotExpression");
            }

            result[0] = m.Groups[1].Value;
            result[1] = m.Groups[3].Value;
            result[2] = m.Groups[2].Value;
            return result;
        }
    }
}
