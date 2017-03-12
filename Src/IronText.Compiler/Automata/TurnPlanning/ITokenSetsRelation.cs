using IronText.Algorithm;

namespace IronText.Automata.TurnPlanning
{
    interface ITokenSetsRelation<T>
    {
        IntSet Of(T position);
    }
}