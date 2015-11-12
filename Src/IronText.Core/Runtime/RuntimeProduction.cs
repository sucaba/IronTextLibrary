using System;

namespace IronText.Runtime
{
    [Serializable]
    public class RuntimeProduction
    {
        public RuntimeProduction(int index, int outcome, int[] input)
        {
            this.Index            = index;
            this.OutcomeToken     = outcome;
            this.Input            = input;
            this.InputLength      = input.Length;
        }

        public int   Index        { get; private set; }

        public int   OutcomeToken { get; private set; }

        public int[] Input        { get; private set; }

        public int   InputLength  { get; private set; }
    }
}
