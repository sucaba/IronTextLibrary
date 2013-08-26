
namespace IronText.Algorithm
{
    public class DecisionTest
    {
        public DecisionTest(IntInterval interval, int action, double elementProbability)
        {
            Interval    = interval;
            Probability = elementProbability * Interval.LongSize;
            Action      = action;
        }

        public readonly IntInterval Interval;
        public readonly int    Action;
        public double Probability;
    }
}
