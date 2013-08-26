using System.Collections.Generic;

namespace IronText.Algorithm
{
    public class UnicodeFrequency : IIntFrequency
    {
        private static UnicodeFrequency _default;
        private readonly MutableIntFrequency data;

        public UnicodeFrequency()
            : this(new MutableIntFrequency())
        {
        }

        private UnicodeFrequency(MutableIntFrequency data)
        {
            this.data = data;
        }

        public IntInterval Bounds { get { return data.Bounds; } }

        public static UnicodeFrequency Default
        {
            get
            {
                if (_default == null)
                {
                    _default = new UnicodeFrequency(BuildDefault());
                }

                return _default;
            }
        }

        public double DefaultValue
        {
            get { return data.DefaultValue; }
        }

        public double Get(int value)
        {
            return data.Get(value);
        }

        public IEnumerable<IntArrow<double>> Enumerate()
        {
            return data.Enumerate();
        }

        private static MutableIntFrequency BuildDefault()
        {
            var result = new MutableIntFrequency();
            result.DefaultValue = 0.0;
            result.Set(new IntArrow<double>(UnicodeIntSetType.UnicodeInterval, 0.0000001));
            result.Set(new IntArrow<double>(0, 1));
            result.Set((IntervalIntSet)UnicodeIntSetType.Instance.LetterNumber, 0.1);
            result.Set((IntervalIntSet)UnicodeIntSetType.Instance.AsciiPrint,   10.0);
            result.Set((IntervalIntSet)UnicodeIntSetType.Instance.AsciiDigit,   20.0);
            result.Set((IntervalIntSet)UnicodeIntSetType.Instance.AsciiAlpha,   100.0);
            result.Normalize();
            return result;
        }

        public double Average(IntInterval interval)
        {
            return data.Average(interval);
        }

        public double Sum(IntInterval interval)
        {
            return data.Sum(interval);
        }

        public IEnumerable<IntArrow<double>> EnumerateCoverage(IntInterval bounds)
        {
            return data.EnumerateCoverage(bounds);
        }
    }
}
