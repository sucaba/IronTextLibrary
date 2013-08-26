using IronText.Framework;

namespace IronText.Runtime
{
    public delegate int TransitionDelegate(int state, int token);

    public delegate int Scan1Delegate(ScanCursor cursor);

    public delegate int ScanActionDelegate(ScanCursor cursor, out object value);

    public delegate IReceiver<Msg> SwitchFactory(
        object         context,
        int            token,
        IReceiver<Msg> exit,
        // TODO: Use ILanguage service instead 
        ILanguage      language);

    public delegate object GrammarActionDelegate(
        BnfRule rule,        // rule being reduced
        Msg[]   parts,       // array containing path being reduced
        int     firstIndex,  // starting index of the path being reduced
        object  context,     // user provided context
        IStackLookback<Msg> lookback    // access to the prior stack states and values
        );

    public delegate object MergeDelegate(
        int     token,
        object  oldValue,
        object  newValue,
        object  context,        // user provided context
        IStackLookback<Msg> lookback   // access to the prior stack states and values
        );
}
