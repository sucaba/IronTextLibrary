using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Runtime
{
    public class RuntimeProduction
    {
        public RuntimeProduction(int index, int outcome, int length)
        {
            this.Index            = index;
            this.OutcomeToken     = outcome;
            this.InputLength      = length;
        }

        public int Index        { get; private set; }

        public int OutcomeToken { get; private set; }

        public int InputLength  { get; private set; }
    }
}
