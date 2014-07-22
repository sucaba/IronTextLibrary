using IronText.Framework;

namespace IronText.Lib.IL
{
    [Demand]
    public interface WantFieldAttrThen<TNext>
    {
        [Produce("static")]
        TNext Static();

        [Produce("public")]
        TNext Public();

        [Produce("private")]
        TNext Private();

        [Produce("family")]
        TNext Family();

        [Produce("initonly")]
        TNext Initonly();

        [Produce("rtspecialname")]
        TNext Rtspecialname();

        [Produce("specialname")]
        TNext Specialname();

#if false
        // TODO: Native Types
        [Parse("marshal", "(", null, ")")] 
        TNext Marshal(nativeType);
#endif
    
        [Produce("assembly")]
        TNext Assembly();

        [Produce("famandassem")]
        TNext Famandassem();

        [Produce("famorassem")]
        TNext Famorassem();

        [Produce("privatescope")]
        TNext Privatescope();

        [Produce("literal")]
        TNext Literal();

        [Produce("notserialized")]
        TNext Notserialized();
    }
}
