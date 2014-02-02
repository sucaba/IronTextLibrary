using IronText.Framework;

namespace CSharpParser
{
    public partial interface ICsGrammar
    {
        [Produce(null, null, null, "struct", null, null, null, null, null, ";")]
        CsStructDeclaration StructDeclaration(
                Opt<CsAttributes>      attributes,
                Opt<CsList<CsStructModifier>> modifier,
                Opt<CsPartial>                partial,
                CsIdentifier                  id,
                Opt<CsTypeParameterList>      typeParameters,
                Opt<CsStructInterfaces>       interfaces,
                Opt<CsList<CsTypeParameterConstraintClause>> typeParameterConstraints,
                CsStructBody                  body);


        [Produce("new")]
        [Produce("public")]
        [Produce("protected")]
        [Produce("internal")]
        [Produce("private")]
        CsStructModifier StructModifier();

        [Produce(":", null)]
        CsStructInterfaces StructInterfaces(CsCommaList<CsInterfaceType> interfaces);

        [Produce("{", null, "}")]
        CsStructBody StructBody(
                Opt<CsList<CsStructMemberDeclaration>> declarations);

        [Produce]
        CsStructMemberDeclaration StructMemberDeclaration(
                CsConstantDeclaration decl);

        [Produce]
        CsStructMemberDeclaration StructMemberDeclaration(
                CsFieldDeclaration decl);

        [Produce]
        CsStructMemberDeclaration StructMemberDeclaration(
                CsMethodDeclaration decl);

        [Produce]
        CsStructMemberDeclaration StructMemberDeclaration(
                CsPropertyDeclaration decl);

        [Produce]
        CsStructMemberDeclaration StructMemberDeclaration(
                CsEventDeclaration decl);

        [Produce]
        CsStructMemberDeclaration StructMemberDeclaration(
                CsIndexerDeclaration decl);

        [Produce]
        CsStructMemberDeclaration StructMemberDeclaration(
                CsOperatorDeclaration decl);

        [Produce]
        CsStructMemberDeclaration StructMemberDeclaration(
                CsConstructorDeclaration decl);

        [Produce]
        CsStructMemberDeclaration StructMemberDeclaration(
                CsStaticConstructorDeclaration decl);

        [Produce]
        CsStructMemberDeclaration StructMemberDeclaration(
                CsTypeDeclaration decl);
    }
}
