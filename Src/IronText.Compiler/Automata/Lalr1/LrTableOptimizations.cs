
namespace IronText.Automata.Lalr1
{
    enum LrTableOptimizations
    {
        None                     = 0x0,
        EliminateLr0ReduceStates = 0x1,

        Default = EliminateLr0ReduceStates,
    }
}
