using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework;

namespace CSharpParser
{
    public partial interface ICsGrammar
    {
        [Parse(null, null, null, "struct", null, null, null, null, null, ";")]
        CsStructDeclaration StructDeclaration(
                Opt<CsAttributes>      attributes,
                CsOptList<CsStructModifier> modifier,
                Opt<CsPartial>                partial,
                CsIdentifier                  id,
                Opt<CsTypeParameterList>      typeParameters,
                Opt<CsStructInterfaces>       interfaces,
                CsOptList<CsTypeParameterConstraintClause> typeParameterConstraints,
                CsStructBody                  body);


        [Parse("new")]
        [Parse("public")]
        [Parse("protected")]
        [Parse("internal")]
        [Parse("private")]
        CsStructModifier StructModifier();

        [Parse(":", null)]
        CsStructInterfaces StructInterfaces(CsCommaList<CsInterfaceType> interfaces);

        [Parse("{", null, "}")]
        CsStructBody StructBody(
                CsOptList<CsStructMemberDeclaration> declarations);

        [Parse]
        CsStructMemberDeclaration StructMemberDeclaration(
                CsConstantDeclaration decl);

        [Parse]
        CsStructMemberDeclaration StructMemberDeclaration(
                CsFieldDeclaration decl);

        [Parse]
        CsStructMemberDeclaration StructMemberDeclaration(
                CsMethodDeclaration decl);

        [Parse]
        CsStructMemberDeclaration StructMemberDeclaration(
                CsPropertyDeclaration decl);

        [Parse]
        CsStructMemberDeclaration StructMemberDeclaration(
                CsEventDeclaration decl);

        [Parse]
        CsStructMemberDeclaration StructMemberDeclaration(
                CsIndexerDeclaration decl);

        [Parse]
        CsStructMemberDeclaration StructMemberDeclaration(
                CsOperatorDeclaration decl);

        [Parse]
        CsStructMemberDeclaration StructMemberDeclaration(
                CsConstructorDeclaration decl);

        [Parse]
        CsStructMemberDeclaration StructMemberDeclaration(
                CsStaticConstructorDeclaration decl);

        [Parse]
        CsStructMemberDeclaration StructMemberDeclaration(
                CsTypeDeclaration decl);
    }
}
