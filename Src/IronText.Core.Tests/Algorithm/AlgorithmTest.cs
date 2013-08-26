using System;
using System.Linq;
using IronText.Algorithm;
using NUnit.Framework;

namespace IronText.Tests.Algorithm
{
    // Hierarchy A
    class BaseA { }
    class ChildA1 : BaseA { }
    class ChildA2 : BaseA { }
    class ChildA1_1 : ChildA1 { }
    class ChildA2_1 : ChildA1 { }

    // Hierarhy B
    class BaseB { }
    class ChildB1 : BaseB { }
    class ChildB2 : BaseB { }
    class ChildB1_1 : ChildB1 { }
    class ChildB2_1 : ChildB1 { }

    [TestFixture]
    public class AlgorithmTest
    {
        [Test]
        public void InheritanceSortTest()
        {
            var input = new[]
            {
                typeof(ChildB1),
                typeof(BaseA),
                typeof(ChildA2),
                typeof(BaseB),
                typeof(ChildB1_1),
                typeof(ChildB2),
                typeof(ChildA1_1),
                typeof(ChildA1),
                typeof(ChildA2_1),
                typeof(ChildB2_1),
            };

            try
            {
                Sorting.SpecializationSort(input);
                for (int i = 0; i != input.Length; ++i)
                    for (int j = i + 1; j != input.Length; ++j)
                    {
                        Assert.IsTrue(IsACmp(input[i], input[j]) >= 0, "i=" + i + ", j=" + j);
                    }
            }
            catch
            {
                Console.WriteLine(string.Join(Environment.NewLine, input.Select(t => t.Name)));
                throw;
            }
        }

        private static int IsACmp(Type x, Type y)
        {
            int result;

            if (y.IsAssignableFrom(x))
            {
                result = 1;
            }
            else if (x.IsAssignableFrom(y))
            {
                result = -1;
            }
            else
            {
                result = 0;
            }

            return result;
        }
    }
}
