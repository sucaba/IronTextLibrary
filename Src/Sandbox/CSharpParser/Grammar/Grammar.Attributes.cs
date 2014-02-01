using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework;

namespace CSharpParser
{
    public partial interface ICsGrammar
    {
        [Produce]
        CsGlobalAttributes GlobalAttributes(
                CsList<CsGlobalAttributeSection> sections);

        [Produce("[", null, ":", null, "]")]
        [Produce("[", null, ":", null, ",", "]")]
        CsGlobalAttributeSection GlobalAttributeSection(
                CsGlobalAttributeTarget  target,
                CsCommaList<CsAttribute> attributes);

        [Produce("assembly")]
        [Produce("module")]
        CsGlobalAttributeTarget GlobalAttributeTarget();

        [Produce]
        CsAttributes Attributes(CsList<CsAttributeSection> sections);

        [Produce("[", null, null, "]")]
        [Produce("[", null, null, ",", "]")]
        CsAttributeSection AttributeSection(
                Opt<CsAttributeTargetSpecifier> specifier,
                CsCommaList<CsAttribute>        attributes);

        [Produce(null, ":")]
        CsAttributeTargetSpecifier AttributeTargetSpecifier(
                CsAttributeTarget target);

        [Produce("field")]
        [Produce("event")]
        [Produce("method")]
        [Produce("param")]
        [Produce("property")]
        [Produce("return")]
        [Produce("type")]
        CsAttributeTarget AttributeTarget();

        [Produce]
        CsAttribute Attribute(
                CsAttributeName           name,
                Opt<CsAttributeArguments> args);

        [Produce]
        CsAttributeName AttributeName(CsTypeName typeName); // also trim "Attribute" suffix

        [Produce("(", null, ")")]
        CsAttributeArguments AttributeArguments(
                Opt<CsCommaList<CsPositionalArgument>> args);

        [Produce("(", null, ",", null, ")")]
        CsAttributeArguments AttributeArguments(
                CsCommaList<CsPositionalArgument> args,
                CsCommaList<CsNamedArgument>      namedArgs);

        [Produce("(", null, ")")]
        CsAttributeArguments AttributeArguments(
                CsCommaList<CsNamedArgument>      namedArgs);

        [Produce]
        CsPositionalArgument PositionalArgument(
                Opt<CsArgumentName>             name,
                CsAttributeArgumentExpression   expression);

        [Produce(null, "=", null)]
        CsNamedArgument NamedArgument(
                CsIdentifier                  identifier,
                CsAttributeArgumentExpression expression);

        [Produce]
        CsAttributeArgumentExpression AttributeArgumentExpression(
                CsExpression expression);
    }
}
