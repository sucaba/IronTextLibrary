using System;
using System.Collections.Generic;
using System.Linq;
using IronText.Algorithm;
using IronText.Lib.IL;
using IronText.Lib.IL.Generators;
using IronText.Tests.TestUtils;
using NUnit.Framework;

namespace IronText.Tests.Lib.IL.Generators
{
    [TestFixture]
    public class SwitchGeneratorTest
    {
        // i - set index
        // k - interval index within a set
        const int Sparse_DefaultValue = -111;
        const int Sparse_PauseSize = 2;
        const int Sparse_SetCount = 10;
        const int Sparse_IntervalsPerSet = 10;
        const int Sparse_IntervalSize = 2;

        private static readonly IntInterval possibleBounds = new IntInterval(
            0, (Sparse_SetCount * Sparse_IntervalsPerSet + 1) * (Sparse_PauseSize + Sparse_IntervalSize));

        [Test]
        public void _0_GenerateContiguousKeysMap()
        {
            int step = 10;
            const int N = 1000;
            int[] map = Enumerable.Range(0, N).Select(i => i*i*step*step).ToArray();
            Func<int, int> nominalMap = x => map[x];

            var emitter = SwitchGenerator.Contiguous(map, -1);
            var cachedMethod = new CachedMethod<Func<int, int>>(
                GetType().FullName + "_Func0",
                (il, args) => 
                {
                    emitter.Build(il, args, SwitchGenerator.RetValueAction);
                    return il;
                });

            for (int i = 0; i != N; ++i)
            {
                Assert.AreEqual(map[i], cachedMethod.Delegate(i));
            }

            Assert.AreEqual(-1, cachedMethod.Delegate(-1));
            Assert.AreEqual(-1, cachedMethod.Delegate(N));
            Assert.AreEqual(-1, cachedMethod.Delegate(N + 1));

            var hash = new Dictionary<int, int>();
            for (int i = 0; i != N; ++i)
            {
                hash[i] = map[i];
            }

            RunBenchmark(
                N / 2,
                nominalMap,
                x => hash[x],
                cachedMethod.Delegate,
                System.Reflection.MethodInfo.GetCurrentMethod().Name);
        }

        [Test, Sequential]
        public void _1_GenerateDecisionSearchMap()
        {
            Func<int, int, IntInterval> getInterval;
            IntArrow<int>[] intervalToValue;
            Func<int, int> nominalMap;
            GenerateSparseIntervalsData(out getInterval, out intervalToValue, out nominalMap);

            var emitter = SwitchGenerator.Sparse(
                            new MutableIntMap<int>(intervalToValue, Sparse_DefaultValue),
                            possibleBounds
                            );

            var cachedMethod = new CachedMethod<Func<int, int>>(
                GetType().FullName + "_Func2_",
                (il, args) => { emitter.Build(il, args, SwitchGenerator.RetValueAction); return il; });

            //for (int trial = 0; trial != 5; ++trial)
            {
                var hash = new Dictionary<int, int>();
                for (int i = 0; i != Sparse_SetCount; ++i)
                {
                    for (int k = 0; k != Sparse_IntervalsPerSet; ++k)
                    {
                        var interval = getInterval(i, k);
                        int[] keys = { interval.First, interval.Last, (interval.First + interval.Last) / 2 };
                        foreach (int key in keys)
                        {
                            Assert.AreEqual(nominalMap(key), cachedMethod.Delegate(key));
                            hash[key] = nominalMap(key);
                        }
                    }
                }

                int perfKey = getInterval(Sparse_SetCount / 2, Sparse_IntervalsPerSet / 2).First;
                RunBenchmark(perfKey, nominalMap, x => hash[x], cachedMethod.Delegate,
                    System.Reflection.MethodInfo.GetCurrentMethod().Name);
            }
        }

        private static void RunBenchmark(
            int perfKey,
            Func<int,int> nominalMap,
            Func<int,int> hashMap,
            Func<int,int> methodMap, 
            string name)
        {
            nominalMap(perfKey);
            constCall(perfKey);
            hashMap(perfKey);
            methodMap(perfKey);

            const int Repeat = 10000000;
            long constCallTime = Bench.DirectMeasure(() => { int ignore = constCall(perfKey); }, Repeat);
            long nominal       = Bench.DirectMeasure(() => { int ignore = nominalMap(perfKey); }, Repeat) - constCallTime;
            long hashMapTime   = Bench.DirectMeasure(() => { int ignore = hashMap(perfKey); }, Repeat) - constCallTime;
            long time          = Bench.DirectMeasure(() => { int ignore = methodMap(perfKey); }, Repeat) - constCallTime;
            Console.WriteLine(name + ":");
            Console.WriteLine(
                "T/array = {0} T/hashMap={1}, call/(T+call)={2}",
                ((double)time) / nominal,
                ((double)time) / hashMapTime,
                ((double)constCallTime) / (time + constCallTime));
        }

        public static int constCall(int _) { return 0; }

        private static void GenerateSparseIntervalsData(out Func<int, int, IntInterval> getInterval, out IntArrow<int>[] intervalToValue, out Func<int, int> nominalMap)
        {
            IntSetType setType = SparseIntSetType.Instance;

            getInterval =
                (i, k) =>
                {
                    int globalIntervalIndex = Sparse_SetCount * k + i;
                    int first = Sparse_PauseSize + (Sparse_PauseSize + Sparse_IntervalSize) * globalIntervalIndex;
                    int last = first + Sparse_IntervalSize - 1;
                    return new IntInterval(first, last);
                };

            intervalToValue = new IntArrow<int>[Sparse_SetCount * Sparse_IntervalsPerSet];

            int maxKey = getInterval(Sparse_SetCount - 1, Sparse_IntervalsPerSet - 1).Last;
            var nominalArray = new int[maxKey + 1];
            for (int i = 0; i != nominalArray.Length; ++i)
            {
                nominalArray[i] = 0; // default values
            }

            for (int i = 0; i != Sparse_SetCount; ++i)
            {
                var mutable = setType.Mutable();
                for (int k = 0; k != Sparse_IntervalsPerSet; ++k)
                {
                    var interval = getInterval(i, k);
                    int value = i + 1; // reserve 0 for default value
                    intervalToValue[i * Sparse_IntervalsPerSet + k] = new IntArrow<int>(interval, value);
                    mutable.Add(interval);

                    int[] keys = { interval.First, interval.Last, (interval.First + interval.Last) / 2 };
                    for (int first = interval.First; first <= interval.Last; ++first)
                    {
                        nominalArray[first] = value;
                    }
                }
            }

            Array.Sort(intervalToValue, (x, y) => x.Key.First - y.Key.First);

            nominalMap = x => nominalArray[x];
        }
    }
}
