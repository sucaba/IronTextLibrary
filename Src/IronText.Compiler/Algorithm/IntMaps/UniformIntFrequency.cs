using System.Collections.Generic;

namespace IronText.Algorithm
{
    public class UniformIntFrequency : IIntFrequency
    {
        private readonly IntArrow<double> arrow;

        public UniformIntFrequency(IntInterval bounds)
        {
            double elementProbability = bounds.IsEmpty ? 0 : (1.0 / bounds.LongSize);
            this.arrow = new IntArrow<double>(bounds, elementProbability);
        }

        public double Sum(IntInterval interval)
        {
            var intersection = arrow * interval;
            if (intersection.IsEmpty)
            {
                return 0;
            }

            double result = intersection.Key.LongSize / (double)arrow.Key.LongSize / interval.LongSize;
            return result;
        }

        public double Average(IntInterval interval)
        {
            var intersection = arrow * interval;
            if (intersection.IsEmpty)
            {
                return 0;
            }

            return intersection.Key.LongSize / (double)arrow.Key.LongSize / interval.LongSize;
        }

        public IntInterval Bounds { get { return arrow.Key; } }

        public double DefaultValue
        {
            get { return 0; }
        }

        public double Get(int value)
        {
            if (arrow.Key.Contains(value))
            {
                return arrow.Value;
            }

            return 0;
        }

        public IEnumerable<IntArrow<double>> Enumerate()
        {
            yield return arrow;
        }

        public IEnumerable<IntArrow<double>> EnumerateCoverage(IntInterval bounds)
        {
            var before = arrow.Before(bounds);
            if (!before.IsEmpty)
            {
                yield return before;
            }
            
            var intersection = arrow * bounds;
            if (!intersection.IsEmpty)
            {
                yield return intersection;
            }

            var after = arrow.After(bounds);
            if (!after.IsEmpty)
            {
                yield return after;
            }
        }
    }
}
