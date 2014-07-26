using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Compiler.Analysis
{
    internal class ProdItem
    {
        public ProdItem(int index, int outcome, IEnumerable<int> input)
            : this(index, outcome, input.ToArray())
        {
        }

        public ProdItem(int index, int outcome, params int[] input)
        {
            this.Index   = index;
            this.Outcome = outcome;
            this.Input   = input;
        }

        public int   Index   { get; private set; }

        public int   Outcome { get; private set; }

        public int[] Input   { get; private set; }
    }
}
