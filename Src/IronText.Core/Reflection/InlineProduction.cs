using System;
using System.Collections.Generic;

namespace IronText.Reflection
{
    public class InlineProduction
    {
        public InlineProduction(int position, Production production)
        {
            this.Position   = position;
            this.Production = production;
            this.Pattern    = (int[])production.InputTokens.Clone();
        }

        /// <summary>
        /// Parent pattern inlining position
        /// </summary>
        public int Position { get; private set; }

        public void Inline(InlineProduction prod)
        {
            int position = prod.Position;

            int inlineLength = prod.Pattern.Length;

            int[] pattern = Pattern;
            Array.Resize(ref pattern, pattern.Length + inlineLength);
            Array.Copy(pattern, position + 1, pattern, position + inlineLength, pattern.Length - position - 1);
            Array.Copy(prod.Pattern, 0, pattern, position, inlineLength);
            this.Pattern = pattern;

            if (Inlines == null)
            {
                Inlines = new List<InlineProduction>();
            }

            Inlines.Add(prod);
        }

        public Production Production { get; private set; }

        /// <summary>
        /// Inlined pattern
        /// </summary>
        public int[] Pattern { get; private set; }

        /// <summary>
        /// Ordered list of inlines
        /// </summary>
        public List<InlineProduction> Inlines { get; private set; }
    }
}
