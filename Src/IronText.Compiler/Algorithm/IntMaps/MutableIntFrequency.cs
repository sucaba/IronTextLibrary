
namespace IronText.Algorithm
{
    public class MutableIntFrequency 
        : MutableIntMap<double>
        , IMutableIntFrequency
    {
        public void Normalize()
        {
            double sum = 0.0;
            foreach (var arrow in arrows)
            {
                sum += arrow.Value * arrow.Key.Size;
            }

            int count = arrows.Count;
            for (int i = 0; i !=  count; ++i)
            {
                var old = arrows[i];
                arrows[i] = new IntArrow<double>(old.Key, old.Value / sum);
            }
        }

        public double Sum(IntInterval bounds)
        {
            double sum = 0;
            foreach (var arrow in EnumerateCoverage(bounds))
            {
                sum += arrow.Value * arrow.Key.LongSize;
            }

            return sum;
        }

        public double Average(IntInterval bounds)
        {
            return Sum(bounds) / bounds.LongSize;
        }
    }
}
