using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework;

namespace CSharpParser
{
    public partial interface ICsGrammar
    {
        [Produce(null, null, null, "interface", null, null, null, null, null, null)]
        CsInterfaceDeclaration InterfaceDeclaration(
                Opt<CsAttributes>                attributes,
                Opt<CsList<CsInterfaceModifier>> modifiers,
                Opt<CsPartial>                   partial,
                CsIdentifier                     id,
                Opt<CsVariantTypeParameterList>  typeParams,
                Opt<CsInterfaceBase>             interfaceBase,
                Opt<CsList<CsTypeParameterConstraintClause>> typeParamsConstraints,
                CsInterfaceBody                  body,
                Opt<CsSemicolon>                 semi);

        [Produce("new")]
        [Produce("public")]
        [Produce("protected")]
        [Produce("internal")]
        [Produce("private")]
        CsInterfaceModifier InterfaceModifier();

        [Produce("<", null, ">")]
        CsVariantTypeParameterList VariantTypeParameterList(
                CsCommaList<CsVariantTypeParameter> prams);

        [Produce]
        CsVariantTypeParameter VariantTypeParameter(
                Opt<CsAttributes>  attributes,
                Opt<CsVarianceAnnotation> variance,
                CsTypeParameter           typeParam);

        [Produce("in")]
        [Produce("out")]
        CsVarianceAnnotation VarianceAnnotation();

        [Produce]
        CsInterfaceBase InterfaceBase(
                CsCommaList<CsInterfaceType> interfaces);

        [Produce("{", null, "}")]
        CsInterfaceBody InterfaceBody(
                Opt<CsList<CsInterfaceMemberDeclaration>> declarations);

        [Produce]
        CsInterfaceMemberDeclaration InterfaceMemberDeclaration(
                CsInterfaceMethodDeclaration decl);

        [Produce]
        CsInterfaceMemberDeclaration InterfaceMemberDeclaration(
                CsInterfacePropertyDeclaration decl);

        [Produce]
        CsInterfaceMemberDeclaration InterfaceMemberDeclaration(
                CsInterfaceEventDeclaration decl);

        [Produce]
        CsInterfaceMemberDeclaration InterfaceMemberDeclaration(
                CsInterfaceIndexerDeclaration decl);

        [Produce(null, null, null, null, null, "(", null, ")", null, ";")]
        CsInterfaceMethodDeclaration InterfaceMethodDeclaration(
                Opt<CsAttributes>   attributes,
                Opt<CsNew>                 @new,
                CsReturnType               returnType,
                CsIdentifier               identifier,
                CsTypeParameterList        typeParams,
                Opt<CsFormalParameterList> formalParams,
                Opt<CsList<CsTypeParameterConstraintClause>> typeParameterConstraints);

        [Produce(null, null, null, null, "{", null, "}")]
        CsInterfacePropertyDeclaration InterfacePropertyDeclaration(
                Opt<CsAttributes>   attributes,
                Opt<CsNew>                 @new,
                CsType                     type,
                CsIdentifier               id,
                CsInterfaceAccessors       accessors);

        [Produce(null, "get", ";")]
        CsInterfaceAccessors InterfaceAccessorsGet(
                Opt<CsAttributes>   attributes);

        [Produce(null, "set", ";")]
        CsInterfaceAccessors InterfaceAccessorsSet(
                Opt<CsAttributes>   attributes);

        [Produce(null, "get", ";", null, "set", ";")]
        CsInterfaceAccessors InterfaceAccessorsGetSet(
                Opt<CsAttributes>   getterAttributes,
                Opt<CsAttributes>   setterAttributes);

        [Produce(null, "set", ";", null, "get", ";")]
        CsInterfaceAccessors InterfaceAccessorsSetGet(
                Opt<CsAttributes>   setterAttributes,
                Opt<CsAttributes>   getterAttributes);

        [Produce(null, null, "event", null, null, ";")]
        CsInterfaceEventDeclaration InterfaceEventDeclaration(
                Opt<CsAttributes>          attributes,
                Opt<CsNew>                 @new,
                CsType                     type,
                CsIdentifier               id);

        [Produce(null, null, null, "this", "[", null, "]", "{", null, "}")]
        CsInterfaceIndexerDeclaration InterfaceIndexerDeclaration(
                Opt<CsAttributes>          attributes,
                Opt<CsNew>                 @new,
                CsType                     type,
                CsFormalParameterList      formalParams,
                CsInterfaceAccessors       accessors);
    }
}
