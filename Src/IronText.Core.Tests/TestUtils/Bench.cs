using System;
using System.Diagnostics;

namespace IronText.Tests.TestUtils
{
    public class Bench
    {
        public static long Measure(Action action, int repeats)
        {
            long result = DirectMeasure(action, repeats);
            long dryRunResult = DirectMeasure(NullAction, repeats);
            return (result - dryRunResult);
        }

        public static long DirectMeasure(Action action, int repeats)
        {
            Stopwatch t = new Stopwatch();
            t.Start();
            //long start = DateTime.Now.Ticks;
            for (int i = 0; i != repeats; ++i)
            {
                action();
            }

            t.Stop();
            return t.ElapsedTicks; //(DateTime.Now.Ticks - start);
        }

        private static void NullAction() { } 
    }
}
