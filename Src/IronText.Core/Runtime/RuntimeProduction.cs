using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Runtime
{
    public class RuntimeProduction
    {
        public RuntimeProduction(int index, int outcome, int[] input)
        {
            this.Index            = index;
            this.OutcomeToken     = outcome;
            this.Input            = input;
        }

        public int   Index        { get; private set; }

        public int   OutcomeToken { get; private set; }

        public int[] Input        { get; private set; }

        public int   InputLength  { get { return Input.Length; } }
    }
}
