using IronText.Framework;

namespace IronText.Lib.IL
{
    [Demand]
    public interface ClassAttrSyntax : ClassIdSyntax
    {
        [ParseGet("public")] 
        ClassAttrSyntax Public { get; }

        [ParseGet("private")] 
        ClassAttrSyntax Private { get; }

        [ParseGet("value")] 
        ClassAttrSyntax Value { get; }

        [ParseGet("enum")] 
        ClassAttrSyntax Enum { get; }

        [ParseGet("interface")] 
        ClassAttrSyntax Interface { get; }

        [ParseGet("sealed")] 
        ClassAttrSyntax Sealed { get; }

        [ParseGet("abstract")] 
        ClassAttrSyntax Abstract { get; }

        [ParseGet("auto")] 
        ClassAttrSyntax Auto { get; }

        [ParseGet("sequential")] 
        ClassAttrSyntax Sequential { get; }

        [ParseGet("explicit")] 
        ClassAttrSyntax Explicit { get; }

        [ParseGet("ansi")] 
        ClassAttrSyntax Ansi { get; }

        [ParseGet("unicode")] 
        ClassAttrSyntax Unicode { get; }

        [ParseGet("autochar")] 
        ClassAttrSyntax Autochar { get; }

        [ParseGet("import")] 
        ClassAttrSyntax Import { get; }

        [ParseGet("serializable")] 
        ClassAttrSyntax Serializable { get; }

        [ParseGet("nested", "public")] 
        ClassAttrSyntax NestedPublic { get; }

        [ParseGet("nested", "private")] 
        ClassAttrSyntax NestedPrivate { get; }

        [ParseGet("nested", "family")] 
        ClassAttrSyntax NestedFamily { get; }

        [ParseGet("nested", "assembly")] 
        ClassAttrSyntax NestedAssembly { get; }

        [ParseGet("nested", "famandassem")] 
        ClassAttrSyntax NestedFamANDAssem { get; }

        [ParseGet("nested", "famorassem")] 
        ClassAttrSyntax NestedFamORAssem { get; }

        [ParseGet("beforefieldinit")] 
        ClassAttrSyntax BeforeFieldInit { get; }

        [ParseGet("specialname")] 
        ClassAttrSyntax SpecialName { get; }

        [ParseGet("rtspecialname")] 
        ClassAttrSyntax RTSpecialName { get; }
    }
}
