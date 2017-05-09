using IronText.Runtime.RIGLR.GraphStructuredStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronText.Runtime
{
    class Reduction<T>
    {
        public Reduction(
            Process<T>        process,
            RuntimeProduction production,
            int               nextState,
            int               leftmostLayer)
        {
            Process       = process;
            Production    = production;
            NextState     = nextState;
            LeftmostLayer = leftmostLayer;
        }

        public bool HasNextState => NextState >= 0;

        public Process<T>        Process       { get; }

        public RuntimeProduction Production    { get; }

        public int               NextState     { get; }

        public int               LeftmostLayer { get; }
    }
}
