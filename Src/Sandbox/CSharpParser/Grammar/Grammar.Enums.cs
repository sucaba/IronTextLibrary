using IronText.Framework;

namespace CSharpParser
{
    public partial interface ICsGrammar
    {
        [Produce(null, null, "enum", null, null, null)]
        CsEnumDeclaration EnumDeclaration(
                Opt<CsAttributes>     attributes,
                Opt<CsList<CsEnumModifier>>  modifiers,
                CsIdentifier                 id,
                Opt<CsEnumBase>              enumBase,
                CsEnumBody                   body,
                Opt<CsSemicolon>             semi);

        [Produce(":", null)]
        CsEnumBase EnumBase(CsIntegralType type);

        [Produce("{", null, "}")]
        CsEnumBody EnumBody(
                Opt<CsCommaList<CsEnumMemberDeclaration>> declarations);

        [Produce("{", null, ",", "}")]
        CsEnumBody EnumBody(
                CsCommaList<CsEnumMemberDeclaration> declarations);

        [Produce("new")]
        [Produce("public")]
        [Produce("protected")]
        [Produce("internal")]
        [Produce("private")]
        CsEnumModifier EnumModifier();

        [Produce]
        CsEnumMemberDeclaration EnumMemberDeclaration(
                Opt<CsAttributes>     attributes,
                CsIdentifier                 id);

        [Produce(null, null, "=", null)]
        CsEnumMemberDeclaration EnumMemberDeclaration(
                Opt<CsAttributes>     attributes,
                CsIdentifier                 id,
                CsConstantExpression         constantExpression);
    }
}
