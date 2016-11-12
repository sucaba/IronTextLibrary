using System;
using System.Linq;
using System.Collections.Generic;
using IronText.Reflection;

namespace IronText.Automata
{
    public class BuildtimeProduction
    {
        public BuildtimeProduction(
            int index,
            int outcome,
            IEnumerable<int> input,
            BuildtimeProductionNode tree)
            : this(index, outcome, input.ToArray(), tree)
        {
        }

        private BuildtimeProduction(
            int index,
            int outcome,
            int[] input,
            BuildtimeProductionNode tree)
        {
            this.Index       = index;
            this.Outcome     = outcome;
            this.Input       = input;
            this.InputLength = input.Length;
            this.Tree        = tree;
        }

        public int   Index        { get; }

        public int   Outcome      { get; }

        public int[] Input        { get; }

        public int   InputLength  { get; }

        public BuildtimeProductionNode Tree { get; }
    }
}
