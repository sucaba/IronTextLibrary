
namespace IronText.Algorithm
{
    public interface IMutableIntFrequency 
        : IIntFrequency
        , IMutableIntMap<double>
    {
        void Normalize();
    }
}
