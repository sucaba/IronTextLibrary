
namespace IronText.Algorithm
{
    public interface IIntFrequency : IIntMap<double>
    {
        double Sum(IntInterval interval);

        double Average(IntInterval interval);
    }
}
