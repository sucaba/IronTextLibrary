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

        [Test]
        public void CallPerformanceTest()
        {
            const int N = 10000000;
            for (int repeat = 3; repeat != 0; --repeat)
            {
                Console.WriteLine("element count = {0}", N);
                Console.WriteLine();

                VClass inst = new DClass();
                VClass baseInst = new VClass();
                ITestContract contr = new TestConforming();
                
                int y = 456;
                var r1 = inst.VirtMethod1(y);
                var r2 = inst.ThisMethod1(y);
                var r3 = VClass.StaticMethod1(y);
                var r4 = inst.VirtFact(5);
                var r5 = inst.ThisFact(5);
                var r6 = VClass.StaticFact(5);
                var r7 = baseInst.VirtFact(5);
                var r8 = contr.MethodImplicit(y);
                var r9 = contr.MethodExplicit(y);
                var r10 = contr.FactExplicit(5);
                var r11 = contr.FactImplicit(5);

                var timer = new Stopwatch();
                timer.Start();
                for (int i = N; i != 0; --i) { }
                timer.Stop();
                TimeSpan dryRun = timer.Elapsed;

                {
                    timer.Reset();
                    timer.Start();
                    for (int i = N; i != 0; --i)
                    {
                        int r = inst.VirtMethod1(y);
                    }
                    timer.Stop();
                    TimeSpan t = (timer.Elapsed - dryRun);
                    Console.WriteLine("virt calls                : {0}", t);
                }

                {
                    timer.Reset();
                    timer.Start();
                    for (int i = N; i != 0; --i)
                    {
                        int r = inst.ThisMethod1(y);
                    }
                    timer.Stop();
                    TimeSpan t = (timer.Elapsed - dryRun);
                    Console.WriteLine("this calls                : {0}", t);
                }

                {
                    timer.Reset();
                    timer.Start();
                    for (int i = N; i != 0; --i)
                    {
                        int r = VClass.StaticMethod1(y);
                    }
                    timer.Stop();
                    TimeSpan t = (timer.Elapsed - dryRun);
                    Console.WriteLine("static calls              : {0}", t);
                }

                int factArg = 10;

                {
                    timer.Reset();
                    timer.Start();
                    for (int i = N; i != 0; --i)
                    {
                        int r = inst.VirtFact(factArg);
                    }
                    timer.Stop();
                    TimeSpan t = (timer.Elapsed - dryRun);
                    Console.WriteLine("derived virt  fact         : {0}", t);
                }

                {
                    timer.Reset();
                    timer.Start();
                    for (int i = N; i != 0; --i)
                    {
                        int r = baseInst.VirtFact(factArg);
                    }
                    timer.Stop();
                    TimeSpan t = (timer.Elapsed - dryRun);
                    Console.WriteLine("base virt  fact           : {0}", t);
                }

                {
                    timer.Reset();
                    timer.Start();
                    for (int i = N; i != 0; --i)
                    {
                        int r = inst.ThisFact(factArg);
                    }
                    timer.Stop();
                    TimeSpan t = (timer.Elapsed - dryRun);
                    Console.WriteLine("this   fact               : {0}", t);
                }

                {
                    timer.Reset();
                    timer.Start();
                    for (int i = N; i != 0; --i)
                    {
                        int r = VClass.StaticFact(factArg);
                    }
                    timer.Stop();
                    TimeSpan t = (timer.Elapsed - dryRun);
                    Console.WriteLine("static fact               : {0}", t);
                }
            }
        }

        class VClass
        {
            public virtual int VirtMethod1(int y) { return y; }

            public int ThisMethod1(int y) { return y; }

            public static int StaticMethod1(int y) { return y; }

            public virtual int VirtFact(int n)
            {
                if (n == 0)
                {
                    return 1;
                }

                return n * VirtFact(n - 1);
            }

            public int ThisFact(int n)
            {
                if (n == 0)
                {
                    return 1;
                }

                return n * ThisFact(n - 1);
            }

            public static int StaticFact(int n)
            {
                if (n == 0)
                {
                    return 1;
                }

                return n * StaticFact(n - 1);
            }
        }

        class DClass : VClass
        {
            public override int VirtMethod1(int y) { return y; }

            public override int VirtFact(int n)
            {
                if (n == 0)
                {
                    return 1;
                }

                return n * VirtFact(n - 1);
            }
        }

        interface ITestContract
        {
            int MethodImplicit(int y);

            int FactImplicit(int n);

            int MethodExplicit(int y);

            int FactExplicit(int n);
        }

        class TestConforming : ITestContract
        {
            public int MethodImplicit(int y) { return y; }

            public int FactImplicit(int n)
            {
                if (n == 0)
                {
                    return 1;
                }

                return n * FactImplicit(n - 1);
            }


            int ITestContract.MethodExplicit(int y)
            {
                return y;
            }

            int ITestContract.FactExplicit(int n)
            {
                return FactImplicit(n);
            }
        }
    }
}
