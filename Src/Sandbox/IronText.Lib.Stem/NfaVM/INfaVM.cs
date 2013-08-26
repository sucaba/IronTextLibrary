using IronText.Framework;
using IronText.Lib.Shared;
using IronText.Lib.Stem;

namespace IronText.Lib.NfaVM
{
    [Language]
    [StaticContext(typeof(Primitives))]
    public interface INfaVM
    {
        [SubContext]
        StemScanner Scanner { get; }

        [SubContext]
        OnDemandNs<Labels> Labels { get; }

        [Parse]
        void Program(Zom_<INfaVM> entries);

        [Parse]
        INfaVM Label(Def<Labels> label);

        /// <summary> Put next input character to the current value register. Initially current value is undefined. </summary>
        [Actor("fetch")]
        INfaVM Fetch();

        [Actor("is")]
        INfaVM IsA(int expected);

        [Actor("match")]
        INfaVM Match();

        [Actor("jmp")]
        INfaVM Jmp(Ref<Labels> label);

        [Actor("fork")]
        INfaVM Fork(Ref<Labels> label);

        [Actor("save")]
        INfaVM Save(int slotIndex);

        [Parse]
        int IntNum(Num num);
    }
}
