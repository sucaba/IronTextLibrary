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
                Opt<CsList<CsAttribute>>         attributes,
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
                Opt<CsList<CsAttribute>>  attributes,
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
                Opt<CsList<CsAttribute>>   attributes,
                Opt<CsNew>                 @new,
                CsReturnType               returnType,
                CsIdentifier               identifier,
                CsTypeParameterList        typeParams,
                Opt<CsFormalParameterList> formalParams,
                Opt<CsList<CsTypeParameterConstraintClause>> typeParameterConstraints);

        [Parse(null, null, null, null, "{", null, "}")]
        CsInterfacePropertyDeclaration InterfacePropertyDeclaration(
                Opt<CsList<CsAttribute>>   attributes,
                Opt<CsNew>                 @new,
                CsType                     type,
                CsIdentifier               id,
                CsInterfaceAccessors       accessors);

        [Parse(null, "get", ";")]
        CsInterfaceAccessors InterfaceAccessorsGet(
                Opt<CsList<CsAttribute>>   attributes);

        [Parse(null, "set", ";")]
        CsInterfaceAccessors InterfaceAccessorsSet(
                Opt<CsList<CsAttribute>>   attributes);

        [Parse(null, "get", ";", null, "set", ";")]
        CsInterfaceAccessors InterfaceAccessorsGetSet(
                Opt<CsList<CsAttribute>>   getterAttributes,
                Opt<CsList<CsAttribute>>   setterAttributes);

        [Parse(null, "set", ";", null, "get", ";")]
        CsInterfaceAccessors InterfaceAccessorsSetGet(
                Opt<CsList<CsAttribute>>   setterAttributes,
                Opt<CsList<CsAttribute>>   getterAttributes);

        [Parse(null, null, "event", null, null, ";")]
        CsInterfaceEventDeclaration InterfaceEventDeclaration(
                Opt<CsList<CsAttribute>>   attributes,
                Opt<CsNew>                 @new,
                CsType                     type,
                CsIdentifier               id);

        [Parse(null, null, null, "this", "[", null, "]", "{", null, "}")]
        CsInterfaceIndexerDeclaration InterfaceIndexerDeclaration(
                Opt<CsList<CsAttribute>>   attributes,
                Opt<CsNew>                 @new,
                CsType                     type,
                CsFormalParameterList      formalParams,
                CsInterfaceAccessors       accessors);
    }
}
