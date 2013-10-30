using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework;

namespace CSharpParser
{
    public partial interface ICsGrammar
    {
        [Parse(null, null, null, "interface", null, null, null, null, null, null)]
        CsInterfaceDeclaration InterfaceDeclaration(
                Opt<CsAttributes>         attributes,
                Opt<CsList<CsInterfaceModifier>> modifiers,
                Opt<CsPartial>                   partial,
                CsIdentifier                     id,
                Opt<CsVariantTypeParameterList>  typeParams,
                Opt<CsInterfaceBase>             interfaceBase,
                Opt<CsList<CsTypeParameterConstraintClause>> typeParamsConstraints,
                CsInterfaceBody                  body,
                Opt<CsSemicolon>                 semi);

        [Parse("new")]
        [Parse("public")]
        [Parse("protected")]
        [Parse("internal")]
        [Parse("private")]
        CsInterfaceModifier InterfaceModifier();

        [Parse("<", null, ">")]
        CsVariantTypeParameterList VariantTypeParameterList(
                CsCommaList<CsVariantTypeParameter> prams);

        [Parse]
        CsVariantTypeParameter VariantTypeParameter(
                Opt<CsAttributes>  attributes,
                Opt<CsVarianceAnnotation> variance,
                CsTypeParameter           typeParam);

        [Parse("in")]
        [Parse("out")]
        CsVarianceAnnotation VarianceAnnotation();

        [Parse]
        CsInterfaceBase InterfaceBase(
                CsCommaList<CsInterfaceType> interfaces);

        [Parse("{", null, "}")]
        CsInterfaceBody InterfaceBody(
                Opt<CsList<CsInterfaceMemberDeclaration>> declarations);

        [Parse]
        CsInterfaceMemberDeclaration InterfaceMemberDeclaration(
                CsInterfaceMethodDeclaration decl);

        [Parse]
        CsInterfaceMemberDeclaration InterfaceMemberDeclaration(
                CsInterfacePropertyDeclaration decl);

        [Parse]
        CsInterfaceMemberDeclaration InterfaceMemberDeclaration(
                CsInterfaceEventDeclaration decl);

        [Parse]
        CsInterfaceMemberDeclaration InterfaceMemberDeclaration(
                CsInterfaceIndexerDeclaration decl);

        [Parse(null, null, null, null, null, "(", null, ")", null, ";")]
        CsInterfaceMethodDeclaration InterfaceMethodDeclaration(
                Opt<CsAttributes>   attributes,
                Opt<CsNew>                 @new,
                CsReturnType               returnType,
                CsIdentifier               identifier,
                CsTypeParameterList        typeParams,
                Opt<CsFormalParameterList> formalParams,
                Opt<CsList<CsTypeParameterConstraintClause>> typeParameterConstraints);

        [Parse(null, null, null, null, "{", null, "}")]
        CsInterfacePropertyDeclaration InterfacePropertyDeclaration(
                Opt<CsAttributes>   attributes,
                Opt<CsNew>                 @new,
                CsType                     type,
                CsIdentifier               id,
                CsInterfaceAccessors       accessors);

        [Parse(null, "get", ";")]
        CsInterfaceAccessors InterfaceAccessorsGet(
                Opt<CsAttributes>   attributes);

        [Parse(null, "set", ";")]
        CsInterfaceAccessors InterfaceAccessorsSet(
                Opt<CsAttributes>   attributes);

        [Parse(null, "get", ";", null, "set", ";")]
        CsInterfaceAccessors InterfaceAccessorsGetSet(
                Opt<CsAttributes>   getterAttributes,
                Opt<CsAttributes>   setterAttributes);

        [Parse(null, "set", ";", null, "get", ";")]
        CsInterfaceAccessors InterfaceAccessorsSetGet(
                Opt<CsAttributes>   setterAttributes,
                Opt<CsAttributes>   getterAttributes);

        [Parse(null, null, "event", null, null, ";")]
        CsInterfaceEventDeclaration InterfaceEventDeclaration(
                Opt<CsAttributes>   attributes,
                Opt<CsNew>                 @new,
                CsType                     type,
                CsIdentifier               id);

        [Parse(null, null, null, "this", "[", null, "]", "{", null, "}")]
        CsInterfaceIndexerDeclaration InterfaceIndexerDeclaration(
                Opt<CsAttributes>   attributes,
                Opt<CsNew>                 @new,
                CsType                     type,
                CsFormalParameterList      formalParams,
                CsInterfaceAccessors       accessors);
    }
}
