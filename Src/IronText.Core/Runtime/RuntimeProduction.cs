using System;
using System.Linq;
using System.Collections.Generic;
using IronText.Reflection;

namespace IronText.Runtime
{
    [Serializable]
    public class RuntimeProduction
    {
        public RuntimeProduction(
            int index,
            int outcome,
            IEnumerable<int> input,
            RuntimeProductionNode tree)
            : this(index, outcome, input.ToArray(), tree)
        {
        }

        private RuntimeProduction(
            int index,
            int outcome,
            int[] input,
            RuntimeProductionNode tree)
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

        public RuntimeProductionNode Tree { get; }
    }
}
