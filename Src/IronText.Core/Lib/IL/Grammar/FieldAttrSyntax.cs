using IronText.Framework;

namespace IronText.Lib.IL
{
    [Demand]
    public interface FieldAttrSyntax
    {
        [Parse("static")]
        FieldAttrSyntax Static();

        [Parse("public")]
        FieldAttrSyntax Public();

        [Parse("private")]
        FieldAttrSyntax Private();

        [Parse("family")]
        FieldAttrSyntax Family();

        [Parse("initonly")]
        FieldAttrSyntax Initonly();

        [Parse("rtspecialname")]
        FieldAttrSyntax Rtspecialname();

        [Parse("specialname")]
        FieldAttrSyntax Specialname();

#if false
        // TODO: Native Types
        [Parse("marshal", "(", null, ")")] 
        FieldAttrSyntax Marshal(nativeType);
#endif
    
        [Parse("assembly")]
        FieldAttrSyntax Assembly();

        [Parse("famandassem")]
        FieldAttrSyntax Famandassem();

        [Parse("famorassem")]
        FieldAttrSyntax Famorassem();

        [Parse("privatescope")]
        FieldAttrSyntax Privatescope();

        [Parse("literal")]
        FieldAttrSyntax Literal();

        [Parse("notserialized")]
        FieldAttrSyntax Notserialized();
    }
}
