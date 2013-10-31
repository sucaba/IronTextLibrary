using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace IronText.Tests.Performance
{
    [TestFixture]
    [Explicit]
    public class ExperimentsTest
    {
        [Test]
        public void EnumeratePerf()
        {
            const int N = 100000000;
            Console.WriteLine("element count = {0}", N);
            Console.WriteLine();

            var list = Enumerable.Range(0, N).ToList();
            var array = new int[list.Count];
            list.CopyTo(array);

            var timer = new Stopwatch();
            timer.Start();
            foreach (var n in list)
            {
            }
            timer.Stop();
            Console.WriteLine("foreach list             : {0}", timer.Elapsed);

            timer.Reset();
            timer.Start();
            for (int i = 0; i != list.Count; ++i)
            {
                var n = list[i];
            }
            timer.Stop();
            Console.WriteLine("for list (count call)    : {0}", timer.Elapsed);

            timer.Reset();
            timer.Start();
            int count = list.Count;
            for (int i = 0; i != count; ++i)
            {
                var n = list[i];
            }
            timer.Stop();
            Console.WriteLine("for list (count variable): {0}", timer.Elapsed);

            timer.Reset();
            timer.Start();
            for (int i = 0; i != N; ++i)
            {
                var n = list[i];
            }
            timer.Stop();
            Console.WriteLine("for list (count const)   : {0}", timer.Elapsed);

            timer.Reset();
            timer.Start();
            foreach (var n in array)
            {
            }
            timer.Stop();
            Console.WriteLine("foreach array            : {0}", timer.Elapsed);

            timer.Reset();
            timer.Start();
            for (int i = 0; i != count; ++i)
            {
                var n = array[i];
            }
            timer.Stop();
            Console.WriteLine("for array                : {0}", timer.Elapsed);
        }

    }
}
