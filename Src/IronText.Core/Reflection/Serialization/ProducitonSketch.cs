using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Reflection
{
    internal class ProductionSketch
    {
        public ProductionSketch(string outcome, params object[] components)
        {
            this.Outcome    = outcome;
            this.Components = components;
        }

        public string   Outcome    { get; private set; }

        public object[] Components { get; private set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as ProductionSketch);
        }

        public bool Equals(ProductionSketch obj)
        {
            return obj != null
                && obj.Outcome == Outcome
                && Enumerable.SequenceEqual(obj.Components, Components);
        }

        public override int GetHashCode()
        {
            return unchecked (Outcome.Length + Components.Length);
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            Write(result);
            return result.ToString();
        }

        public void Write(StringBuilder output)
        {
            output
                .Append(Outcome)
                .Append(" = ");

            foreach (var component in Components)
            {
                output.Append(' ');
                var sketch = component as ProductionSketch;
                if (sketch == null)
                {
                    output.Append((string)component);
                }
                else
                {
                    output.Append('(');
                    sketch.Write(output);
                    output.Append(')');
                }
            }
        }

        public static ProductionSketch Parse(string text)
        {
            int pos = 0;
            int end = text.Length;

            var result = ParseProduction(text, ref pos, end);

            SkipSpaces(text, ref pos, end);
            if (pos != end)
            {
                var msg = string.Format("error ({0}): Unexpected character '{1}'.", pos + 1, text[pos]);
                throw new InvalidOperationException(msg);
            }

            return result;
        }

        private static object ParseComponent(string text, ref int pos, int end)
        {
            if (pos == end)
            {
                return null;
            }

            if (text[pos] == '(')
            {
                ++pos;
                var result = ParseProduction(text, ref pos, end);

                if (pos != end )
                {
                    if (text[pos] != ')')
                    {
                        var msg = string.Format("error({0}): Missing closing paren.", pos + 1);
                        throw new InvalidOperationException(msg);
                    }

                    ++pos;
                }

                return result;
            }

            return ParseName(text, ref pos, end);
        }

        private static ProductionSketch ParseProduction(string text, ref int pos, int end)
        {
            string outcome = ParseName(text, ref pos, end);
            if (outcome == null)
            {
                var msg = string.Format("error({0}): Unable to parse symbol.", pos + 1);
                throw new InvalidOperationException(msg);
            }

            SkipSpaces(text, ref pos, end);
            MatchSeparator(text, ref pos, end);

            object component;
            var components = new List<object>();
            do
            {
                SkipSpaces(text, ref pos, end);
                component = ParseComponent(text, ref pos, end);
                if (component == null)
                {
                    break;
                }

                components.Add(component);
            }
            while (true);
            var result = new ProductionSketch(outcome, components.ToArray());
            return result;
        }

        private static void SkipSpaces(string text, ref int pos, int end)
        {
            while (pos != end && char.IsWhiteSpace(text[pos]))
            {
                ++pos;
            }
        }

        private static void MatchSeparator(string text, ref int pos, int end)
        {
            if (pos == end)
            {
                var msg = string.Format("error ({0}): Unexpected end of input.", pos + 1);
                throw new InvalidOperationException(msg);
            }

            char ch = text[pos];
            if (ch != ':' && ch != '=')
            {
                var msg = string.Format("error ({0}): Unexpected character '{1}'.", pos + 1, text[pos]);
                throw new InvalidOperationException(msg);
            }

            ++pos;
        }

        private static string ParseName(string text, ref int pos, int end)
        {
            int start = pos;
            while (pos != end && (char.IsLetterOrDigit(text[pos]) || "?'$_<>-[]".Contains(text[pos])))
            {
                ++pos;
            }

            if (start == pos)
            {
                return null;
            }

            return text.Substring(start, (pos - start));
        }
    }
}
