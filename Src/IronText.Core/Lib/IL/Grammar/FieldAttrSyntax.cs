using IronText.Framework;

namespace IronText.Lib.IL
{
    [Demand]
    public interface FieldAttrSyntax
    {
        [Produce("static")]
        FieldAttrSyntax Static();

        [Produce("public")]
        FieldAttrSyntax Public();

        [Produce("private")]
        FieldAttrSyntax Private();

        [Produce("family")]
        FieldAttrSyntax Family();

        [Produce("initonly")]
        FieldAttrSyntax Initonly();

        [Produce("rtspecialname")]
        FieldAttrSyntax Rtspecialname();

        [Produce("specialname")]
        FieldAttrSyntax Specialname();

#if false
        // TODO: Native Types
        [Parse("marshal", "(", null, ")")] 
        FieldAttrSyntax Marshal(nativeType);
#endif
    
        [Produce("assembly")]
        FieldAttrSyntax Assembly();

        [Produce("famandassem")]
        FieldAttrSyntax Famandassem();

        [Produce("famorassem")]
        FieldAttrSyntax Famorassem();

        [Produce("privatescope")]
        FieldAttrSyntax Privatescope();

        [Produce("literal")]
        FieldAttrSyntax Literal();

        [Produce("notserialized")]
        FieldAttrSyntax Notserialized();
    }
}
