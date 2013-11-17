using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Framework.Reflection
{
    public class InlineProduction
    {
        private Dictionary<int, InlineProduction> inlines;

        public InlineProduction(Production production)
        {
            this.Production = production;
            this.Pattern = (int[])production.Pattern.Clone();
        }

        public void Inline(int index, InlineProduction inlined)
        {
            int inlineLength = inlined.Pattern.Length;
            int[] pattern = Pattern;
            Array.Resize(ref pattern, pattern.Length + inlineLength);
            Array.Copy(pattern, index + 1, pattern, index + inlineLength, pattern.Length - index - 1);
            Array.Copy(inlined.Pattern, 0, pattern, index, inlineLength);
            Pattern = pattern;

            if (inlines == null)
            {
                inlines = new Dictionary<int, InlineProduction>();
            }

            inlines.Add(index, inlined);
        }

        public Production Production { get; private set; }

        /// <summary>
        /// Inlined pattern
        /// </summary>
        public int[] Pattern { get; private set; }
    }
}
