namespace IronText.Automata.Lalr1
{
    public enum LrTableOptimizations
    {
        None                     = 0x0,
        EliminateLr0ReduceStates = 0x1,

        Default                  = None
    }
}
